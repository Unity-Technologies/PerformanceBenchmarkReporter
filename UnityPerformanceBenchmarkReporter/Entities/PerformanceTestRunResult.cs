using System;
using System.Collections.Generic;

namespace UnityPerformanceBenchmarkReporter.Entities
{
    [Serializable]
    public class PerformanceTestRunResult
    {
        public PlayerSystemInfo PlayerSystemInfo;
        public EditorVersion EditorVersion;
        public BuildSettings BuildSettings;
        public ScreenSettings ScreenSettings;
        public QualitySettings QualitySettings;
        public PlayerSettings PlayerSettings;
        public string TestSuite;
        public DateTime StartTime;
        public List<TestResult> TestResults  = new List<TestResult>();
        public bool IsBaseline;
        public string ResultName;

        public bool TestRunMetadataExists()
        {
            return PlayerSystemInfo != null
                   || PlayerSettings != null
                   || QualitySettings != null
                   || ScreenSettings != null
                   || BuildSettings != null
                   || EditorVersion != null;
        }
    }
}