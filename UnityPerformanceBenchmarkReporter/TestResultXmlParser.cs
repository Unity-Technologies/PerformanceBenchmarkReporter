using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Newtonsoft.Json;
using UnityPerformanceBenchmarkReporter.Entities;
using UnityPerformanceBenchmarkReporter.Entities.New;

namespace UnityPerformanceBenchmarkReporter
{
    public class TestResultXmlParser : IParser
    {
        public PerformanceTestRun Parse(string path,int version)
        {
            var xmlDocument = XDocument.Load(path);
            return Parse(xmlDocument);
        }

        private static PerformanceTestRun Parse(XDocument xmlDocument)
        {
            var output = xmlDocument.Descendants("output");
            var xElements = output as XElement[] ?? output.ToArray();

            if (!xElements.Any())
            {
                return null;
            }

            var run = DeserializeMetadata(xElements) ?? DeserializeMetadataV2(xElements);

            if (run == null)
            {
                return null;
            }

            run.EndTime = DateTime.Now.ToUniversalTime()
                .Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                .TotalMilliseconds;

            DeserializeTestResults(xElements, run);
            DeserializeTestResultsV2(xElements, run);

            return run;
        }

        private static void DeserializeTestResults(IEnumerable<XElement> output, PerformanceTestRun run)
        {
            foreach (var element in output)
            {
                foreach (var line in element.Value.Split('\n'))
                {
                    var json = GetJsonFromHashtag("performancetestresult", line);
                    if (json == null) continue;

                    var result = JsonConvert.DeserializeObject<PerformanceTestResult>(json);
                    run.Results.Add(result);
                }
            }
        }

        private static void DeserializeTestResultsV2(IEnumerable<XElement> output, PerformanceTestRun run)
        {
            foreach (var element in output)
            {
                foreach (var line in element.Value.Split('\n'))
                {
                    var json = GetJsonFromHashtag("performancetestresult2", line);
                    if (json == null)
                    {
                        continue;
                    }

                    var result = TryDeserializePerformanceTestResultJsonObject(json);
                    if (result != null)
                    {
                        var pt = new PerformanceTestResult()
                        {
                            TestCategories = result.Categories,
                            TestName = result.Name,
                            TestVersion = result.Version,
                            SampleGroups = result.SampleGroups.Select(sg => new Entities.SampleGroup
                            {
                                Samples = sg.Samples,
                                Average = sg.Average,
                                Max = sg.Max,
                                Median = sg.Median,
                                Min = sg.Min,
                                Sum = sg.Sum,
                                StandardDeviation = sg.StandardDeviation,
                                SampleCount = sg.Samples.Count,
                                
                                Definition = new SampleGroupDefinition()
                                {
                                    Name = sg.Name,
                                    SampleUnit = (Entities.SampleUnit)sg.Unit,
                                    IncreaseIsBetter = sg.IncreaseIsBetter,
                                    Threshold = sg.Threshold
                                    ContainsKnownIssue = sg.ContainsKnownIssue,
                                    KnownIssueDetails = sg.KnownIssueDetails
                                }
                            }).ToList()
                        };
                        run.Results.Add(pt);
                    }
                }
            }
        }

        private static PerformanceTestRun DeserializeMetadata(IEnumerable<XElement> output)
        {
            return (output
                .SelectMany(element => element.Value.Split('\n'),
                    (element, line) => GetJsonFromHashtag("performancetestruninfo", line))
                .Where(json => json != null)
                .Select(JsonConvert.DeserializeObject<PerformanceTestRun>)).FirstOrDefault();
        }

        private static PerformanceTestRun DeserializeMetadataV2(IEnumerable<XElement> output)
        {
            foreach (var element in output)
            {
                var pattern = @"##performancetestruninfo2:(.+)\n";
                var regex = new Regex(pattern);
                var matches = regex.Match(element.Value);
                if (!matches.Success) continue;
                if (matches.Groups.Count == 0) continue;

                if (matches.Groups[1].Captures.Count > 1)
                {
                    throw new Exception("Performance test run had multiple hardware and player settings, there should only be one.");
                }

                var json = matches.Groups[1].Value;
                if (string.IsNullOrEmpty(json))
                {
                    throw new Exception("Performance test run has incomplete hardware and player settings.");
                }

                var result = TryDeserializePerformanceTestRunJsonObject(json);

                var run = new PerformanceTestRun()
                {
                    BuildSettings = new BuildSettings()
                    {
                        Platform = result.Player.Platform,
                        BuildTarget = result.Player.BuildTarget,
                        DevelopmentPlayer = true,
                        AndroidBuildSystem = result.Player.AndroidBuildSystem
                    },
                    EditorVersion = new EditorVersion()
                    {
                        Branch = result.Editor.Branch,
                        DateSeconds = result.Editor.Date,
                        FullVersion = $"{result.Editor.Version} ({result.Editor.Changeset})",
                        RevisionValue = 0
                    },
                    PlayerSettings = new PlayerSettings()
                    {
                        GpuSkinning = result.Player.GpuSkinning,
                        GraphicsApi = result.Player.GraphicsApi,
                        RenderThreadingMode = result.Player.RenderThreadingMode,
                        ScriptingBackend = result.Player.ScriptingBackend,
                        AndroidTargetSdkVersion = result.Player.AndroidTargetSdkVersion,
                        EnabledXrTargets = new List<string>(),
                        ScriptingRuntimeVersion = "",
                        StereoRenderingPath = result.Player.StereoRenderingPath
                    },
                    QualitySettings = new QualitySettings()
                    {
                        Vsync = result.Player.Vsync,
                        AntiAliasing = result.Player.AntiAliasing,
                        AnisotropicFiltering = result.Player.AnisotropicFiltering,
                        BlendWeights = result.Player.BlendWeights,
                        ColorSpace = result.Player.ColorSpace
                    },
                    ScreenSettings = new ScreenSettings()
                    {
                        Fullscreen = result.Player.Fullscreen,
                        ScreenHeight = result.Player.ScreenHeight,
                        ScreenWidth = result.Player.ScreenWidth,
                        ScreenRefreshRate = result.Player.ScreenRefreshRate
                    },
                    PlayerSystemInfo = new Entities.PlayerSystemInfo()
                    {
                        DeviceModel = result.Hardware.DeviceModel,
                        DeviceName = result.Hardware.DeviceName,
                        OperatingSystem = result.Hardware.OperatingSystem,
                        ProcessorCount = result.Hardware.ProcessorCount,
                        ProcessorType = result.Hardware.ProcessorType,
                        GraphicsDeviceName = result.Hardware.GraphicsDeviceName,
                        SystemMemorySize = result.Hardware.SystemMemorySizeMB,
                        XrDevice = "",
                        XrModel = ""
                    },
                    StartTime = result.Date,
                    TestSuite = result.TestSuite,
                    Results = new List<PerformanceTestResult>()
                };

                return run;
            }

            return null;
        }

        private static Run TryDeserializePerformanceTestRunJsonObject(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<Run>(json);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return null;
        }

        private static Entities.New.TestResult TryDeserializePerformanceTestResultJsonObject(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<Entities.New.TestResult>(json);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return null;
        }

        private static string GetJsonFromHashtag(string tag, string line)
        {
            if (!line.Contains($"##{tag}:")) return null;
            var jsonStart = line.IndexOf('{');
            var openBrackets = 0;
            var stringIndex = jsonStart;
            while (openBrackets > 0 || stringIndex == jsonStart)
            {
                var character = line[stringIndex];
                if (character == '{')
                {
                    openBrackets++;
                }

                if (character == '}')
                {
                    openBrackets--;
                }

                stringIndex++;
            }

            var jsonEnd = stringIndex;
            return line.Substring(jsonStart, jsonEnd - jsonStart);
        }
    }
}
