using System;
using System.Collections.Generic;

namespace UnityPerformanceBenchmarkReporter.Entities
{
    [Serializable]
    public class PerformanceTestResult
    {
        public string TestName;
        public List<string> TestCategories;
        public string TestVersion;
        public double StartTime;
        public double EndTime;
        public List<SampleGroup> SampleGroups;
    }
}