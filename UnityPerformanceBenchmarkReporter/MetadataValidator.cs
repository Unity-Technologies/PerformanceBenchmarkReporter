using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityPerformanceBenchmarkReporter.Entities;

namespace UnityPerformanceBenchmarkReporter
{
    internal class MetadataValidator
    {
        private string[] playerSystemInfoResultFiles = Array.Empty<string>();
        private Dictionary<string, Dictionary<string, string>> mismatchedPlayerSystemInfoValues = new Dictionary<string, Dictionary<string, string>>();

        public void ValidatePlayerSystemInfo(PerformanceTestRun testRun1, PerformanceTestRun testRun2, string firstTestRunResultPath, string xmlFileNamePath)
        {
            if (testRun1.PlayerSystemInfo.DeviceModel != testRun2.PlayerSystemInfo.DeviceModel)
            {
                WriteWarningMessage("Test results with mismatching PlayerSystemInfo.DeviceModeltestRun");
            }
            if (testRun1.PlayerSystemInfo.GraphicsDeviceName != testRun2.PlayerSystemInfo.GraphicsDeviceName)
            {
                WriteWarningMessage("Test results with mismatching PlayerSystemInfo.GraphicsDeviceName");
            }
            if (testRun1.PlayerSystemInfo.ProcessorCount != testRun2.PlayerSystemInfo.ProcessorCount)
            {
                WriteWarningMessage("Test results with mismatching PlayerSystemInfo.ProcessorCount");
            }
            if (testRun1.PlayerSystemInfo.ProcessorType != testRun2.PlayerSystemInfo.ProcessorType)
            {
                WriteWarningMessage("Test results with mismatching PlayerSystemInfo.ProcessorType");
            }
            if (testRun1.PlayerSystemInfo.XrDevice != testRun2.PlayerSystemInfo.XrDevice)
            {
                AddMismatchPlayerInfo(
                    ref playerSystemInfoResultFiles, 
                    mismatchedPlayerSystemInfoValues, 
                    firstTestRunResultPath, 
                    xmlFileNamePath, 
                    nameof(testRun1.PlayerSystemInfo.XrDevice), 
                    testRun1.PlayerSystemInfo.XrDevice, 
                    testRun2.PlayerSystemInfo.XrDevice);
            }
            if (testRun1.PlayerSystemInfo.XrModel != testRun2.PlayerSystemInfo.XrModel)
            {
                AddMismatchPlayerInfo(
                    ref playerSystemInfoResultFiles, 
                    mismatchedPlayerSystemInfoValues, 
                    firstTestRunResultPath, 
                    xmlFileNamePath, 
                    nameof(testRun1.PlayerSystemInfo.XrModel), 
                    testRun1.PlayerSystemInfo.XrModel, 
                    testRun2.PlayerSystemInfo.XrModel);
            }

            if (mismatchedPlayerSystemInfoValues.Count > 0)
            {
                WriteWarningMessage(
                    string.Format("Test results with mismatched {0}", "SystemInfo PlayerSettings"), 
                    playerSystemInfoResultFiles,
                    mismatchedPlayerSystemInfoValues);
            }
        }

        private void AddMismatchPlayerInfo(
            ref string[] infoResultFiles,
            Dictionary<string, Dictionary<string, string>> mismatchedPlayerValues,
            string firstTestRunResultPath,
            string xmlFileNamePath,
            string playerInfoName,
            string refPlayerInfoValue,
            string mismatchedPlayerInfoValue)
        {
            EnsureResultFileTracked(ref infoResultFiles, firstTestRunResultPath);
            EnsureResultFileTracked(ref infoResultFiles, xmlFileNamePath);

            if (!mismatchedPlayerValues.ContainsKey(playerInfoName))
            {
                mismatchedPlayerValues.Add(
                    playerInfoName,
                    new Dictionary<string, string>
                    {
                        {firstTestRunResultPath, refPlayerInfoValue}
                    });
            }

            mismatchedPlayerValues[playerInfoName].Add(xmlFileNamePath, mismatchedPlayerInfoValue ?? string.Empty);
        }

        private void EnsureResultFileTracked(ref string[] infoResultFiles, string resultPath)
        {
            if (!infoResultFiles.Contains(resultPath))
            {
                Array.Resize(ref infoResultFiles, infoResultFiles.Length + 1);
                infoResultFiles[infoResultFiles.Length - 1] = resultPath;
            }
        }

        public void ValidatePlayerSettings(PerformanceTestRun testRun1, PerformanceTestRun testRun2, string firstTestRunResultPath, string xmlFileNamePath)
        {
            if (testRun1.PlayerSettings.AndroidMinimumSdkVersion != testRun2.PlayerSettings.AndroidMinimumSdkVersion)
            {
                WriteWarningMessage("Test results with mismatching PlayerSettings.AndroidMinimumSdkVersion");
            }
            if (testRun1.PlayerSettings.AndroidTargetSdkVersion != testRun2.PlayerSettings.AndroidTargetSdkVersion)
            {
                WriteWarningMessage("Test results with mismatching PlayerSettings.GraphicsDeviceName");
            }
            foreach (var enabledXrTarget in testRun1.PlayerSettings.EnabledXrTargets)
            {
                if (!testRun2.PlayerSettings.EnabledXrTargets.Contains(enabledXrTarget))
                {
                    WriteWarningMessage("Test results with mismatching PlayerSettings.EnabledXrTargets");  
                }
            }
            if (testRun1.PlayerSettings.GpuSkinning != testRun2.PlayerSettings.GpuSkinning)
            {
                WriteWarningMessage("Test results with mismatching PlayerSettings.GpuSkinning");
            }
            if (testRun1.PlayerSettings.GraphicsApi != testRun2.PlayerSettings.GraphicsApi)
            {
                WriteWarningMessage("Test results with mismatching PlayerSettings.GraphicsApi");
            }
            if (testRun1.PlayerSettings.GraphicsJobs != testRun2.PlayerSettings.GraphicsJobs)
            {
                WriteWarningMessage("Test results with mismatching PlayerSettings.GraphicsJobs");
            }
            if (testRun1.PlayerSettings.MtRendering != testRun2.PlayerSettings.MtRendering)
            {
                WriteWarningMessage("Test results with mismatching PlayerSettings.MtRendering");
            }
            if (testRun1.PlayerSettings.RenderThreadingMode != testRun2.PlayerSettings.RenderThreadingMode)
            {
                WriteWarningMessage("Test results with mismatching PlayerSettings.RenderThreadingMode");
            }
            if (testRun1.PlayerSettings.ScriptingBackend != testRun2.PlayerSettings.ScriptingBackend)
            {
                WriteWarningMessage("Test results with mismatching PlayerSettings.ScriptingBackend");
            }
            if (testRun1.PlayerSettings.StereoRenderingPath != testRun2.PlayerSettings.StereoRenderingPath)
            {
                WriteWarningMessage("Test results with mismatching PlayerSettings.StereoRenderingPath");
            }
            if (testRun1.PlayerSettings.VrSupported != testRun2.PlayerSettings.VrSupported)
            {
                WriteWarningMessage("Test results with mismatching PlayerSettings.VrSupported");
            }
        }

        public void ValidateQualitySettings(PerformanceTestRun testRun1, PerformanceTestRun testRun2, string firstTestRunResultPath, string xmlFileNamePath)
        {
            if (testRun1.QualitySettings.AnisotropicFiltering != testRun2.QualitySettings.AnisotropicFiltering)
            {
                WriteWarningMessage("Test results with mismatching QualitySettings.AnisotropicFiltering");
            }
            if (testRun1.QualitySettings.AntiAliasing != testRun2.QualitySettings.AntiAliasing)
            {
                WriteWarningMessage("Test results with mismatching QualitySettings.AntiAliasing");
            }
            if (testRun1.QualitySettings.BlendWeights != testRun2.QualitySettings.BlendWeights)
            {
                WriteWarningMessage("Test results with mismatching QualitySettings.BlendWeights");
            }
            if (testRun1.QualitySettings.ColorSpace != testRun2.QualitySettings.ColorSpace)
            {
                WriteWarningMessage("Test results with mismatching QualitySettings.ColorSpace");
            }
            if (testRun1.QualitySettings.Vsync != testRun2.QualitySettings.Vsync)
            {
                WriteWarningMessage("Test results with mismatching QualitySettings.Vsync");
            }
        }

        public void ValidateScreenSettings(PerformanceTestRun testRun1, PerformanceTestRun testRun2, string firstTestRunResultPath, string xmlFileNamePath)
        {
            if (testRun1.ScreenSettings.Fullscreen != testRun2.ScreenSettings.Fullscreen)
            {
                WriteWarningMessage("Test results with mismatching ScreenSettings.Fullscreen");
            }
            if (testRun1.ScreenSettings.ScreenHeight != testRun2.ScreenSettings.ScreenHeight)
            {
                WriteWarningMessage("Test results with mismatching ScreenSettings.ScreenHeight");
            }
            if (testRun1.ScreenSettings.ScreenRefreshRate != testRun2.ScreenSettings.ScreenRefreshRate)
            {
                WriteWarningMessage("Test results with mismatching ScreenSettings.ScreenRefreshRate");
            }
            if (testRun1.ScreenSettings.ScreenWidth != testRun2.ScreenSettings.ScreenWidth)
            {
                WriteWarningMessage("Test results with mismatching ScreenSettings.ScreenWidth");
            }
        }

        private void WriteWarningMessage(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        private void WriteWarningMessage(string msg, string[] playerResultFiles, Dictionary<string, Dictionary<string, string>> mismatchedPlayerValues)
        {
            var sb = new StringBuilder();

            sb.Append(msg + "\r\n");
            sb.Append(GetLine());
            var tableHeader = new string[playerResultFiles.Length + 1];
            tableHeader[0] = "Name";
            playerResultFiles.CopyTo(tableHeader, 1);
            sb.Append(GetRow(tableHeader));
            sb.Append(GetLine());

            foreach (var mismatchPlayerValue in mismatchedPlayerValues)
            {
                string [] rowData = new string[tableHeader.Length];
                rowData[0] = mismatchPlayerValue.Key;

                for (int i = 0; i < playerResultFiles.Length; i++)
                {
                    if (mismatchPlayerValue.Value.Any(v => v.Key.Equals(playerResultFiles[i])))
                    {
                        rowData[i + 1] = mismatchPlayerValue.Value.First(v => v.Key.Equals(playerResultFiles[i])).Value;
                    }
                    else
                    {
                        rowData[i + 1] = string.Empty;
                    }
                }

                sb.Append(GetRow(rowData));
            }

            sb.Append(GetLine());
            WriteWarningMessage(sb.ToString());
        }

        static int tableWidth = 100;

        static string GetLine()
        {
            return string.Format("{0}\r\n", new string('-', tableWidth));
        }

        static string GetRow(params string[] columns)
        {
            int width = (tableWidth - columns.Length) / columns.Length;
            string row = "|";

            foreach (string column in columns)
            {
                row += AlignCentre(column, width) + "|";
            }

            return string.Format("{0}\r\n", row);
        }

        static string AlignCentre(string alignText, int width)
        {
            var text = alignText;
            if (text.Length > width)
            {
                var startIndex = text.Length - width + 3;
                var length = width - 3;
                var substring = text.Substring(startIndex, length); 
                text = "..." + substring;
            }
            else
                text = text;

            if (string.IsNullOrEmpty(text))
            {
                return new string(' ', width);
            }
            else
            {
                return text.PadRight(width - (width - text.Length) / 2).PadLeft(width);
            }
        }
    }
}
