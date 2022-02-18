using UnityPerformanceBenchmarkReporter.Entities;

namespace UnityPerformanceBenchmarkReporter
{
    public interface IParser
    {
        public PerformanceTestRun Parse(string path);
    }
}