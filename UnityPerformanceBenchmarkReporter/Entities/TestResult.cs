using System.Collections.Generic;

namespace UnityPerformanceBenchmarkReporter.Entities
{
    public class TestResult
    {
        public string TestName;
        public List<string> TestCategories;
        public string TestVersion;
        public int State;
        public List<SampleGroupResult> SampleGroupResults;
    }
}
