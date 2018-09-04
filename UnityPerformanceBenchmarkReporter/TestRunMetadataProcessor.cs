using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using UnityPerformanceBenchmarkReporter.Entities;

namespace UnityPerformanceBenchmarkReporter
{
    public class TypeMetadata
    {
        public TypeMetadata(Type thisType)
        {
            Type = thisType;
        }

        public List<FieldGroup> FieldGroups { get; } = new List<FieldGroup>();

        public int NullResultCount { get; set; }

        public int ValidResultCount { get; set; }

        public bool HasMismatches
        {
            get { return FieldGroups.Any(g => g.HasMismatches); }
        }

        public Type Type { get; }

        public string TypeName => Type.Name;
    }

    public class FieldGroup
    {
        // using an array instead of list so we can preserve ordering for display purposes
        public FieldValue[] Values = new FieldValue[0];

        public FieldGroup(string fieldName)
        {
            FieldName = fieldName;
        }

        public string FieldName { get; }

        public bool HasMismatches
        {
            get { return Values.Any(v => v.IsMismatched); }
        }
    }

    public class FieldValue
    {
        private readonly char pathSeperator = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? '\\' : '/';

        public bool IsMismatched;

        public FieldValue(string resultFilePath, string value)
        {
            if (!string.IsNullOrEmpty(resultFilePath))
            {
                var pathParts = resultFilePath.Split(pathSeperator);
                ResultFileDirectory = string.Join(pathSeperator, pathParts.Take(pathParts.Length - 1));
                ResultFileName = pathParts[pathParts.Length - 1];
            }
            
            Value = value;
        }

        public string Value { get; }
        public string ResultFileDirectory { get; }

        public string ResultFileName { get; }
    }

    public class TestRunMetadataProcessor
    {
        private readonly string[] androidOnlyMetadata =
        {
            "AndroidMinimumSdkVersion",
            "AndroidTargetSdkVersion",
            "AndroidBuildSystem"
        };

        private readonly Dictionary<Type, string[]> excludedConfigFieldNames;

        private readonly Type[] metadataTypes =
        {
            typeof(PlayerSystemInfo),
            typeof(PlayerSettings),
            typeof(ScreenSettings),
            typeof(QualitySettings),
            typeof(BuildSettings),
            typeof(EditorVersion)
        };

        public readonly List<TypeMetadata> TypeMetadata = new List<TypeMetadata>();

        private readonly string[] xrOnlyMetadata =
        {
            "XrModel",
            "XrDevice",
            "EnabledXrTargets",
            "StereoRenderingPath"
        };


        public TestRunMetadataProcessor(Dictionary<Type, string[]> excludedFieldNames)
        {
            excludedConfigFieldNames = excludedFieldNames;
        }

        private bool isVrSupported;
        private bool isAndroid;

        /// <summary>
        ///     Assumes first performanceTestRun should be used to compare all other performanceTestRuns against.
        /// </summary>
        /// <param name="performanceTestRun"></param>
        /// <param name="xmlFileNamePath"></param>
        public void ProcessMetadata(PerformanceTestRun performanceTestRun, string xmlFileNamePath)
        {
            SetIsVrSupported(new[] {performanceTestRun});
            SetIsAndroid(new[] {performanceTestRun});

            foreach (var metadataType in metadataTypes)
            {
                var typeMetadata = TypeMetadata.Any(tm => tm.Type == metadataType)
                    ? TypeMetadata.First(m => m.Type == metadataType)
                    : null;

                // If metadataType doesn't exist in our TypeMetadata list, add it
                if (typeMetadata == null)
                {
                    typeMetadata = new TypeMetadata(metadataType);
                    TypeMetadata.Add(typeMetadata);
                }

                var fieldInfos = performanceTestRun.GetType().GetFields();
                // If this metadataType is completely missing from the perf test run, mark it as such and move on
                if (fieldInfos.Any(f => f.FieldType == metadataType))
                {
                    var fieldInfo = fieldInfos.First(f => f.FieldType == metadataType);

                    object obj = null;
                    GetFieldInfoValue(performanceTestRun, metadataType, ref obj, fieldInfo);

                    // If null, we're missing metadata for this performanceTestRun
                    if (obj == null)
                    {
                        typeMetadata.NullResultCount++;

                        // But we already have results for this metadataType,
                        // add an empty "missing value" entry for it each FieldGroup
                        if (typeMetadata.ValidResultCount > 0)
                        {
                            foreach (var fieldGroup in typeMetadata.FieldGroups)
                            {
                                BackfillFieldGroupValuesForMissingMetadata(xmlFileNamePath, fieldGroup, typeMetadata);
                            }
                        }
                        continue;
                    }
                    var fields = obj.GetType().GetFields();
                    var fieldsToProcess = GetFieldsToProcess(metadataType, fields);

                    // if we have valid field metadata to process
                    if (fieldsToProcess.Length > 0)
                    {
                        foreach (var field in fieldsToProcess)
                        {
                            if (!typeMetadata.FieldGroups.Any(fg => fg.FieldName.Equals(field.Name)))
                            {
                                typeMetadata.FieldGroups.Add(new FieldGroup(field.Name));
                            }

                            var thisFieldGroup = typeMetadata.FieldGroups.First(fg => fg.FieldName.Equals(field.Name));

                            // We want to keep the values array length consistent with the number of results, even for results
                            // that are missing metadata. We do that here.
                            BackfillFieldGroupValuesForMissingMetadata(xmlFileNamePath, thisFieldGroup, typeMetadata);

                            // Add this field value
                            var value = GetValueBasedOnType(metadataType, field, obj);

                            Array.Resize(ref thisFieldGroup.Values, thisFieldGroup.Values.Length + 1);
                            thisFieldGroup.Values[thisFieldGroup.Values.Length - 1] = new FieldValue(xmlFileNamePath, value);

                            // fieldGroup.Values is sorted by ascending execution start time; the first element in this array
                            // is considered to be the reference point, regardless if it's a "baseline" or not.
                            if (thisFieldGroup.Values[thisFieldGroup.Values.Length - 1].Value != thisFieldGroup.Values[0].Value)
                            {
                                thisFieldGroup.Values[thisFieldGroup.Values.Length - 1].IsMismatched = true;
                            }
                        }

                        typeMetadata.ValidResultCount++;
                    }
                }
            }
        }

        private void BackfillFieldGroupValuesForMissingMetadata(string xmlFileNamePath, FieldGroup fieldGroup,
            TypeMetadata typeMetadata)
        {
            if (fieldGroup.Values.Length < typeMetadata.ValidResultCount + typeMetadata.NullResultCount)
            {
                while (fieldGroup.Values.Length < typeMetadata.ValidResultCount + typeMetadata.NullResultCount)
                {
                    Array.Resize(ref fieldGroup.Values, fieldGroup.Values.Length + 1);
                    fieldGroup.Values[fieldGroup.Values.Length - 1] = new FieldValue(xmlFileNamePath, "Metadata not available");

                    // fieldGroup.Values is sorted by ascending execution start time; the first element in this array
                    // is considered to be the reference point, regardless if it's a "baseline" or not.
                    if (fieldGroup.Values[fieldGroup.Values.Length - 1].Value != fieldGroup.Values[0].Value)
                    {
                        fieldGroup.Values[fieldGroup.Values.Length - 1].IsMismatched = true;
                    }
                }
            }
        }

        private void GetFieldInfoValue(PerformanceTestRun performanceTestRun, Type metadataType, ref object obj,
            FieldInfo fieldInfo)
        {
            if (metadataType == typeof(PlayerSystemInfo))
            {
                obj = (PlayerSystemInfo) fieldInfo.GetValue(performanceTestRun);
            }

            if (metadataType == typeof(PlayerSettings))
            {
                obj = (PlayerSettings) fieldInfo.GetValue(performanceTestRun);
            }

            if (metadataType == typeof(ScreenSettings))
            {
                obj = (ScreenSettings) fieldInfo.GetValue(performanceTestRun);
            }

            if (metadataType == typeof(QualitySettings))
            {
                obj = (QualitySettings) fieldInfo.GetValue(performanceTestRun);
            }

            if (metadataType == typeof(BuildSettings))
            {
                obj = (BuildSettings) fieldInfo.GetValue(performanceTestRun);
            }

            if (metadataType == typeof(EditorVersion))
            {
                obj = (EditorVersion) fieldInfo.GetValue(performanceTestRun);
            }
        }

        private string GetValueBasedOnType(Type metadataType, FieldInfo field, object obj)
        {
            string value = null;
            if (metadataType == typeof(PlayerSystemInfo))
            {
                value = GetValue<PlayerSystemInfo>(field, obj);
            }

            if (metadataType == typeof(PlayerSettings))
            {
                value = GetValue<PlayerSettings>(field, obj);
            }

            if (metadataType == typeof(ScreenSettings))
            {
                value = GetValue<ScreenSettings>(field, obj);
            }

            if (metadataType == typeof(QualitySettings))
            {
                value = GetValue<QualitySettings>(field, obj);
            }

            if (metadataType == typeof(BuildSettings))
            {
                value = GetValue<BuildSettings>(field, obj);
            }

            if (metadataType == typeof(EditorVersion))
            {
                value = GetValue<EditorVersion>(field, obj);
            }

            return value;
        }

        private string GetValue<T>(FieldInfo field, object obj)
        {
            if (IsIEnumerableFieldType(field))
                return ConvertIEnumberableToString(field, (T) obj);
            var value = field.GetValue((T) obj);
            if (value.GetType() != typeof(string))
            {
                value = Convert.ToString(value);
            }

            return (string) value;
        }

        private FieldInfo[] GetFieldsToProcess(Type metadataType, FieldInfo[] fields)
        {
// Derive a subset of fields to process from validFieldNames
            var excludedFieldNames = excludedConfigFieldNames.ContainsKey(metadataType)
                ? excludedConfigFieldNames[metadataType]
                : null;
            var validFieldNames = GetValidFieldNames(excludedFieldNames, fields.Select(f => f.Name).ToArray());
            var fieldsToProcess = fields.Join(validFieldNames, f => f.Name, s => s, (field, vField) => field).ToArray();
            return fieldsToProcess;
        }

        private string ConvertIEnumberableToString<T>(FieldInfo field, T thisObject)
        {
            var sb = new StringBuilder();
            var fieldValues = (List<string>) (IEnumerable) field.GetValue(thisObject);

            if (fieldValues != null && fieldValues.Any())
            {
                foreach (var enumerable in fieldValues)
                {
                    sb.Append(enumerable + ",");
                }

                if (sb.ToString().EndsWith(','))
                {
                    // trim trailing comma
                    sb.Length--;
                }
            }
            else
            {
                sb.Append("None");
            }

            return sb.ToString();
        }

        private bool IsIEnumerableFieldType(FieldInfo field)
        {
            return typeof(IEnumerable).IsAssignableFrom(field.FieldType) && field.FieldType != typeof(string);
        }

        private string[] GetValidFieldNames(string[] excludedFieldNames, IEnumerable<string> fieldNames)
        {
            var validFieldNames = fieldNames as string[] ?? fieldNames.ToArray();

            if (excludedFieldNames != null && excludedFieldNames.Any())
            {
                validFieldNames = validFieldNames.Where(k1 => excludedFieldNames.All(k2 => k2 != k1)).ToArray();
            }

            if (!isVrSupported)
            {
                validFieldNames = validFieldNames.Where(k1 => xrOnlyMetadata.All(k2 => k2 != k1)).ToArray();
            }

            if (!isAndroid)
            {
                validFieldNames = validFieldNames.Where(k1 => androidOnlyMetadata.All(k2 => k2 != k1)).ToArray();
            }

            return validFieldNames;
        }

        private void SetIsVrSupported(PerformanceTestRun[] performanceTestRuns)
        {
            foreach (var performanceTestRun in performanceTestRuns)
            {
                isVrSupported = isVrSupported || performanceTestRun.PlayerSettings != null &&
                                performanceTestRun.PlayerSettings.EnabledXrTargets.Any();
            }
        }

        private void SetIsAndroid(PerformanceTestRun[] performanceTestRuns)
        {
            foreach (var performanceTestRun in performanceTestRuns)
            {
                isAndroid = isAndroid || performanceTestRun.BuildSettings != null &&
                            performanceTestRun.BuildSettings.Platform.Equals("Android");
            }
        }
    }
}