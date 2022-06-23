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
    public class TestResultJsonParser : IParser
    {
        public PerformanceTestRun Parse(string path, int version)
        {
            string report = "";
            try
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    string stream = reader.ReadToEnd().Trim();

                    // json wrawrapped in invalid [], removed for valid json format 
                    if (stream[0] == '[' && stream[stream.Length - 1] == ']')
                        report = stream.Substring(1, stream.Length - 2);
                    else
                        report = stream;
                }
            }
            catch (System.Exception)
            {

                throw;
            }

            switch (version)
            {
                case 1:
                    return ParseJsonV1(report);
                case 2:
                    return ParseJsonV2(report);
                default:
                    return null;
            }
        }

        private static PerformanceTestRun ParseJsonV1(string json)
        {

            PerformanceTestRun result;

            try
            {
                result = JsonConvert.DeserializeObject<PerformanceTestRun>(json);
                var path = result.PlayerSettings.StereoRenderingPath;
                result.PlayerSettings.StereoRenderingPath = GetStereoPath(path,result.PlayerSettings.AndroidTargetSdkVersion);
            }
            catch (System.Exception)
            {

                throw;
            }


            return result;
        }

        private static PerformanceTestRun ParseJsonV2(string json)
        {

            Run run = null;
            try
            {
                run = JsonConvert.DeserializeObject<Run>(json);
            }
            catch (System.Exception)
            {
                throw;
            }

            if (run != null)
            {
                var testRun = new PerformanceTestRun()
                {
                    BuildSettings = new BuildSettings()
                    {
                        Platform = run.Player.Platform,
                        BuildTarget = run.Player.BuildTarget,
                        DevelopmentPlayer = true,
                        AndroidBuildSystem = run.Player.AndroidBuildSystem
                    },
                    EditorVersion = new EditorVersion()
                    {
                        Branch = run.Editor.Branch,
                        DateSeconds = run.Editor.Date,
                        FullVersion = $"{run.Editor.Version} ({run.Editor.Changeset})",
                        RevisionValue = 0
                    },
                    PlayerSettings = new PlayerSettings()
                    {
                        GpuSkinning = run.Player.GpuSkinning,
                        GraphicsApi = run.Player.GraphicsApi,
                        RenderThreadingMode = run.Player.RenderThreadingMode,
                        ScriptingBackend = run.Player.ScriptingBackend,
                        AndroidTargetSdkVersion = run.Player.AndroidTargetSdkVersion,
                        EnabledXrTargets = new List<string>(),
                        ScriptingRuntimeVersion = "",
                        StereoRenderingPath = GetStereoPath(run.Player.StereoRenderingPath, run.Player.AndroidTargetSdkVersion)
                    },
                    QualitySettings = new QualitySettings()
                    {
                        Vsync = run.Player.Vsync,
                        AntiAliasing = run.Player.AntiAliasing,
                        AnisotropicFiltering = run.Player.AnisotropicFiltering,
                        BlendWeights = run.Player.BlendWeights,
                        ColorSpace = run.Player.ColorSpace
                    },
                    ScreenSettings = new ScreenSettings()
                    {
                        Fullscreen = run.Player.Fullscreen,
                        ScreenHeight = run.Player.ScreenHeight,
                        ScreenWidth = run.Player.ScreenWidth,
                        ScreenRefreshRate = run.Player.ScreenRefreshRate
                    },
                    PlayerSystemInfo = new Entities.PlayerSystemInfo()
                    {
                        DeviceModel = run.Hardware.DeviceModel,
                        DeviceName = run.Hardware.DeviceName,
                        OperatingSystem = run.Hardware.OperatingSystem,
                        ProcessorCount = run.Hardware.ProcessorCount,
                        ProcessorType = run.Hardware.ProcessorType,
                        GraphicsDeviceName = run.Hardware.GraphicsDeviceName,
                        SystemMemorySize = run.Hardware.SystemMemorySizeMB,
                        XrDevice = run.Hardware.XrDevice,
                        XrModel = run.Hardware.XrModel
                    },
                    StartTime = run.Date,
                    TestSuite = run.TestSuite,
                    Results = new List<PerformanceTestResult>()
                };

                testRun.EndTime = DateTime.Now.ToUniversalTime()
                .Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                .TotalMilliseconds;

                foreach (var res in run.Results)
                {
                    var pt = new PerformanceTestResult()
                    {
                        TestCategories = res.Categories,
                        TestName = res.Name,
                        TestVersion = res.Version,
                        SampleGroups = res.SampleGroups.Select(sg => new Entities.SampleGroup
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
                            }
                        }).ToList()
                    };
                    testRun.Results.Add(pt);
                }

                return testRun;
            }

            return null;
        }

        /// This allows us to get the stereo mode from a custom data string as well as the normal stereo mode setting
        /// This is due to a bug in the perf framework where the stereo mode always displays multipass
        /// We pass this custom string in as AndroidTargetSDKVersion
        /// The second parameter takes in this string compares the values and overwrites the stereo mode if the mode in the custom string eists and is different
        private static string GetStereoPath(string stereoModeString, string miscDataString)
        {

            if(string.IsNullOrWhiteSpace(stereoModeString))
            {
                Console.WriteLine("Stereo Mode String Is Null");
                return "";
            }
                

            Regex stereoModeRegex = new Regex(@"stereorenderingmode=([^|]*)");
            var match  = stereoModeRegex.Match(miscDataString);
            if(match.Success)
            {

                if(match.Groups.Count <= 1)
                    return stereoModeString;

                if(stereoModeString.ToLower() == match.Value.ToLower())
                {
                    return stereoModeString;
                }
                else
                {
                    return match.Groups[1].Value;
                }
            }
            else
            {
                Console.WriteLine("Failed To Parse Custom Data String for StereoMode");
                return stereoModeString;
            }

            
        }
    }
}
