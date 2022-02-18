using System;
using System.Collections.Generic;

namespace UnityPerformanceBenchmarkReporter.Entities
{
    [Serializable]
    public class PerformanceTestRun
    {
        public PlayerSystemInfo PlayerSystemInfo;
        public EditorVersion EditorVersion;
        public BuildSettings BuildSettings;
        public ScreenSettings ScreenSettings;
        public QualitySettings QualitySettings;
        public PlayerSettings PlayerSettings;
        public ProjectVersion ProjectVersion;

        public string TestProject;
        public string TestSuite;
        public double StartTime;
        public double EndTime;
        public List<PerformanceTestResult> Results  = new List<PerformanceTestResult>();

        public JobMetaData JobMetaData ;
        public object Dependencies;
    }

    [Serializable]    
        public class ProjectVersion
    {
        public string ProjectName { get; set; }
        public string Branch { get; set; }
        public string Changeset { get; set; }
        public DateTime Date { get; set; }
    }

    [Serializable]
    public class PlayerSystemInfo
    {
        public string OperatingSystem;
        public string DeviceModel;
        public string DeviceName;
        public string ProcessorType;
        public int ProcessorCount;
        public string GraphicsDeviceName;
        public int SystemMemorySize;
        public string XrModel;
        public string XrDevice;
    }

    [Serializable]
    public class EditorVersion
    {
        public string FullVersion;
        public int DateSeconds;
        public string Branch;
        public int RevisionValue;
    }

    [Serializable]
    public class BuildSettings
    {
        public string Platform;
        public string BuildTarget;
        public bool DevelopmentPlayer;
        public string AndroidBuildSystem;
    }

    [Serializable]
    public class ScreenSettings
    {
        public int ScreenWidth;
        public int ScreenHeight;
        public int ScreenRefreshRate;
        public bool Fullscreen;
    }

    [Serializable]
    public class QualitySettings
    {
        public int Vsync;
        public int AntiAliasing;
        public string ColorSpace;
        public string AnisotropicFiltering;
        public string BlendWeights;
    }

    [Serializable]
    public class PlayerSettings
    {
        public string ScriptingBackend;
        public bool VrSupported;
        public bool MtRendering;
        public bool GraphicsJobs;
        public bool GpuSkinning;
        public string GraphicsApi;
        public string Batchmode; 
        //public int StaticBatching; TODO
        //public int DynamicBatching; TODO
        public string StereoRenderingPath;
        public string RenderThreadingMode;
        public string AndroidMinimumSdkVersion;
        public string AndroidTargetSdkVersion;
        public List<string> EnabledXrTargets;

        public string ScriptingRuntimeVersion;
    }
    

    [Serializable]
     public class Yamato
    {
        public string JobFriendlyName { get; set; }
        public string JobName { get; set; }
        public string JobId { get; set; }
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public object WorkDir { get; set; }
        public object JobOwnerEmail { get; set; }
    }

    [Serializable]
    public class JobMetaData
    {
        public Yamato Yamato { get; set; }
        public object Bokken { get; set; }
    }
}