using System;
using UnityPerformanceBenchmarkReporter.Entities;

namespace UnityPerformanceBenchmarkReporter
{
    internal class MetadataValidator
    {
        public void ValidatePlayerSystemInfo(PerformanceTestRun testRun1, PerformanceTestRun testRun2)
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
                WriteWarningMessage("Test results with mismatching PlayerSystemInfo.XrDevice");
            }
            if (testRun1.PlayerSystemInfo.XrModel != testRun2.PlayerSystemInfo.XrModel)
            {
                WriteWarningMessage("Test results with mismatching PlayerSystemInfo.XrModel");
            }
        }

        public void ValidatePlayerSettings(PerformanceTestRun testRun1, PerformanceTestRun testRun2)
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

        public void ValidateQualitySettings(PerformanceTestRun testRun1, PerformanceTestRun testRun2)
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

        public void ValidateScreenSettings(PerformanceTestRun testRun1, PerformanceTestRun testRun2)
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
    }
}
