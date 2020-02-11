using System;
using System.Collections.Generic;

namespace UnityPerformanceBenchmarkReporter.Entities.New
{
    [Serializable]
    public class TestResult
    {
        public string Name;
        public string Version;
        public List<string> Categories = new List<string>();
        public List<SampleGroup> SampleGroups = new List<SampleGroup>();
    }

    [Serializable]
    public class SampleGroup
    {
        public string Name;
        public SampleUnit Unit;
        public bool IncreaseIsBetter;
        public List<double> Samples = new List<double>();
        public double Min;
        public double Max;
        public double Median;
        public double Average;
        public double StandardDeviation;
        public double Sum;

        public SampleGroup(string name, SampleUnit unit, bool increaseIsBetter)
        {
            Name = name;
            Unit = unit;
            IncreaseIsBetter = increaseIsBetter;
        }
    }

    [Serializable]
    public class Run
    {
        public string TestSuite;
        public int Date;
        public Player Player;
        public Hardware Hardware;
        public Editor Editor;
        public List<string> Dependencies = new List<string>();
        public List<TestResult> Results = new List<TestResult>();
    }

    [Serializable]
    public class Editor
    {
        public string Version;
        public string Branch;
        public string Changeset;
        public int Date;
    }

    [Serializable]
    public class Hardware
    {
        public string OperatingSystem;
        public string DeviceModel;
        public string DeviceName;
        public string ProcessorType;
        public int ProcessorCount;
        public string GraphicsDeviceName;
        public int SystemMemorySizeMB;
    }

    [Serializable]
    public class Player
    {
        public string Platform;
        public bool Development;
        public int ScreenWidth;
        public int ScreenHeight;
        public int ScreenRefreshRate;
        public bool Fullscreen;
        public int Vsync;
        public int AntiAliasing;
        public string ColorSpace;
        public string AnisotropicFiltering;
        public string BlendWeights;
        public string GraphicsApi;
        public bool Batchmode;
        public string RenderThreadingMode;
        public bool GpuSkinning;

        // strings because values are editor only enums
        public string ScriptingBackend;
        public string AndroidTargetSdkVersion;
        public string AndroidBuildSystem;
        public string BuildTarget;
        public string StereoRenderingPath;
    }

    public enum SampleUnit
    {
        Nanosecond,
        Microsecond,
        Millisecond,
        Second,
        Byte,
        Kilobyte,
        Megabyte,
        Gigabyte,
        Undefined
    }
}
