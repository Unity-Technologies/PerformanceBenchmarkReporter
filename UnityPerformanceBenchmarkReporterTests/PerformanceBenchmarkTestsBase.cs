using System.IO;
using UnityPerformanceBenchmarkReporter;

namespace UnityPerformanceBenchmarkReporterTests
{
    public class PerformanceBenchmarkTestsBase
    {
        protected PerformanceBenchmark PerformanceBenchmark;
        private static readonly string TestDataDirectoryName = "TestData";

        protected string EnsureFullPath(string directoryOrFileName)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            return !directoryOrFileName.StartsWith(TestDataDirectoryName)
                ? Path.Combine(currentDirectory, TestDataDirectoryName, directoryOrFileName)
                : Path.Combine(currentDirectory, directoryOrFileName);
        }
    }
}