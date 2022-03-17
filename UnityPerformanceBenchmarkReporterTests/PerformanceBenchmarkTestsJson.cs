using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityPerformanceBenchmarkReporter;
using UnityPerformanceBenchmarkReporter.Entities;

namespace UnityPerformanceBenchmarkReporterTests
{
    public class PerformanceBenchmarkTestsJson : PerformanceBenchmarkTestsBase
    {
        private OptionsParser optionsParser;
        private IParser testResultJsonParser;
        private List<PerformanceTestRunResult> performanceTestRunResults;
        private List<TestResult> testResults;
        private List<PerformanceTestRunResult> baselinePerformanceTestRunResults;
        private List<TestResult> baselineTestResults;

        [SetUp]
        public void Setup()
        {
            optionsParser = new OptionsParser();
            PerformanceBenchmark = new PerformanceBenchmark();
            testResultJsonParser = new TestResultJsonParser();
            performanceTestRunResults = new List<PerformanceTestRunResult>();
            testResults = new List<TestResult>();
            baselinePerformanceTestRunResults = new List<PerformanceTestRunResult>();
            baselineTestResults = new List<TestResult>();
        }


        [Test]
        public void VerifyV1_AddPerformanceTestRunResults()
        {
            // Arrange
            var resultJsonFilePath = EnsureFullPath("results.Json");
            var args = new[] { "--format=Json",string.Format("--testresultsxmlsource={0}", resultJsonFilePath), "--version=1" };
            optionsParser.ParseOptions(PerformanceBenchmark, args);

            // Act
            PerformanceBenchmark.AddPerformanceTestRunResults(testResultJsonParser, performanceTestRunResults, testResults, new List<TestResult>());

            // Assert
            Assert.IsTrue(PerformanceBenchmark.ResultFilesExist);
            AssertCorrectResultJsonFilePaths(new[] { resultJsonFilePath });
            Assert.NotNull(testResults);
            Assert.IsTrue(testResults.Count > 0);
            Assert.NotNull(performanceTestRunResults);
            Assert.IsTrue(performanceTestRunResults.Count > 0);
        }

        [Test]
        public void VerifyV1_AddPerformanceTestRunResults_TwoResultFiles()
        {
            // Arrange
            var resultJsonFilePath = EnsureFullPath("results.Json");
            var resultFileName2 = EnsureFullPath("results2.Json");
            var args = new[]
            {
                "--format=Json",
                string.Format("--testresultsxmlsource={0}", resultJsonFilePath),
                string.Format("--testresultsxmlsource={0}", resultFileName2)
                , "--version=1"
            };
            optionsParser.ParseOptions(PerformanceBenchmark, args);

            // Act
            PerformanceBenchmark.AddPerformanceTestRunResults(testResultJsonParser, performanceTestRunResults, testResults, new List<TestResult>());

            // Assert
            Assert.IsTrue(PerformanceBenchmark.ResultFilesExist);
            AssertCorrectResultJsonFilePaths(new[] { resultJsonFilePath, resultFileName2 });
            Assert.NotNull(testResults);
            Assert.IsTrue(testResults.Count > 0);
            Assert.NotNull(performanceTestRunResults);
            Assert.IsTrue(performanceTestRunResults.Count > 0);
        }

        [Test]
        public void VerifyV1_AddPerformanceTestRunResults_OneResultFiles_OneResultDirectory()
        {
            // Arrange
            var resultJsonFilePath = EnsureFullPath("results.Json");
            var resultsJsonDir = EnsureFullPath("ResultsJson");
            var args = new[]
            {
                "--format=Json",
                    string.Format("--testresultsxmlsource={0}", resultJsonFilePath),
                    string.Format("--testresultsxmlsource={0}", resultsJsonDir)
                    , "--version=1"
            };
            optionsParser.ParseOptions(PerformanceBenchmark, args);

            // Act
            PerformanceBenchmark.AddPerformanceTestRunResults(testResultJsonParser, performanceTestRunResults, testResults, new List<TestResult>());

            // Assert
            Assert.IsTrue(PerformanceBenchmark.ResultFilesExist);
            AssertCorrectResultsJsonDirectoryPaths(new[] { resultsJsonDir });
            AssertCorrectResultJsonFilePaths(new[] { resultJsonFilePath });
            Assert.NotNull(testResults);
            Assert.IsTrue(testResults.Count > 0);
            Assert.NotNull(performanceTestRunResults);
            Assert.IsTrue(performanceTestRunResults.Count > 0);
        }

        [Test]
        public void VerifyV1_AddBaselinePerformanceTestRunResults()
        {
            // Arrange
            var resultJsonFilePath = EnsureFullPath("results.Json");
            var baselineJsonFilePath = EnsureFullPath("baseline.Json");
            var args = new[]
            {
                "--format=Json",
                string.Format("--testresultsxmlsource={0}", resultJsonFilePath),
                string.Format("--baselinexmlsource={0}", baselineJsonFilePath)
                , "--version=1"
            };
            optionsParser.ParseOptions(PerformanceBenchmark, args);

            // Act
            PerformanceBenchmark.AddBaselinePerformanceTestRunResults(testResultJsonParser, baselinePerformanceTestRunResults, baselineTestResults);

            // Assert
            Assert.IsTrue(PerformanceBenchmark.BaselineResultFilesExist);
            AssertCorrectBaselineJsonFilePaths(new[] { baselineJsonFilePath });
            AssertCorrectResultJsonFilePaths(new[] { resultJsonFilePath });
            Assert.NotNull(baselineTestResults);
            Assert.IsTrue(baselineTestResults.Count > 0);
            Assert.NotNull(baselinePerformanceTestRunResults);
            Assert.IsTrue(baselinePerformanceTestRunResults.Count > 0);
        }

        [Test]
        public void VerifyV1_AddBaselinePerformanceTestRunResultsDirectory()
        {
            // Arrange
            var resultJsonFilePath = EnsureFullPath("ResultsJson");
            var baselineJsonFilePath = EnsureFullPath("BaselineJson");
            var args = new[]
            {
                "--format=Json",
                string.Format("--testresultsxmlsource={0}", resultJsonFilePath),
                string.Format("--baselinexmlsource={0}", baselineJsonFilePath)
                , "--version=1"
            };
            optionsParser.ParseOptions(PerformanceBenchmark, args);

            // Act
            PerformanceBenchmark.AddBaselinePerformanceTestRunResults(testResultJsonParser, baselinePerformanceTestRunResults, baselineTestResults);

            // Assert
            Assert.IsTrue(PerformanceBenchmark.BaselineResultFilesExist);
            Assert.NotNull(baselineTestResults);
            Assert.IsTrue(baselineTestResults.Count > 0);
            Assert.NotNull(baselinePerformanceTestRunResults);
            Assert.IsTrue(baselinePerformanceTestRunResults.Count > 0);
        }

        [Test]
        public void VerifyV1_Verify_AddBaselineAndNonBaselinePerformanceTestRunResults()
        {
            // Arrange
            var resultJsonFilePath = EnsureFullPath("results.Json");
            var baselineJsonFilePath = EnsureFullPath("baseline.Json");
            var args = new[]
            {
                "--format=Json",
                    string.Format("--testresultsxmlsource={0}", resultJsonFilePath),
                    string.Format("--baselinexmlsource={0}", baselineJsonFilePath)
                    , "--version=1"
                };
            optionsParser.ParseOptions(PerformanceBenchmark, args);

            // Act
            PerformanceBenchmark.AddBaselinePerformanceTestRunResults(testResultJsonParser, baselinePerformanceTestRunResults, baselineTestResults);
            PerformanceBenchmark.AddPerformanceTestRunResults(testResultJsonParser, performanceTestRunResults, testResults, new List<TestResult>());

            // Assert
            Assert.IsTrue(PerformanceBenchmark.ResultFilesExist);
            AssertCorrectResultJsonFilePaths(new[] { resultJsonFilePath });
            Assert.NotNull(testResults);
            Assert.IsTrue(testResults.Count > 0);
            Assert.NotNull(performanceTestRunResults);
            Assert.IsTrue(performanceTestRunResults.Count > 0);

            Assert.IsTrue(PerformanceBenchmark.BaselineResultFilesExist);
            AssertCorrectBaselineJsonFilePaths(new[] { baselineJsonFilePath });
            Assert.NotNull(baselineTestResults);
            Assert.IsTrue(baselineTestResults.Count > 0);
            Assert.NotNull(baselinePerformanceTestRunResults);
            Assert.IsTrue(baselinePerformanceTestRunResults.Count > 0);
        }

     public void VerifyV2_AddPerformanceTestRunResults()
        {
            // Arrange
            var resultJsonFilePath = EnsureFullPath("baseline2.Json");
            var args = new[] { string.Format("--format=Json","--testresultsxmlsource={0}", resultJsonFilePath), "--version=2" };
            optionsParser.ParseOptions(PerformanceBenchmark, args);

            // Act
            PerformanceBenchmark.AddPerformanceTestRunResults(testResultJsonParser, performanceTestRunResults, testResults, new List<TestResult>());

            // Assert
            Assert.IsTrue(PerformanceBenchmark.ResultFilesExist);
            AssertCorrectResultJsonFilePaths(new[] { resultJsonFilePath });
            Assert.NotNull(testResults);
            Assert.IsTrue(testResults.Count > 0);
            Assert.NotNull(performanceTestRunResults);
            Assert.IsTrue(performanceTestRunResults.Count > 0);
        }

        [Test]
        public void VerifyV2_AddPerformanceTestRunResults_TwoResultFiles()
        {
            // Arrange
            var resultJsonFilePath = EnsureFullPath("ResultsJson2/results2.Json");
            var resultFileName2 = EnsureFullPath("results2.Json");
            var args = new[]
            {
                "--format=Json",
                string.Format("--testresultsxmlsource={0}", resultJsonFilePath),
                string.Format("--testresultsxmlsource={0}", resultFileName2)
                , "--version=2"
            };
            optionsParser.ParseOptions(PerformanceBenchmark, args);

            // Act
            PerformanceBenchmark.AddPerformanceTestRunResults(testResultJsonParser, performanceTestRunResults, testResults, new List<TestResult>());

            // Assert
            Assert.IsTrue(PerformanceBenchmark.ResultFilesExist);
            AssertCorrectResultJsonFilePaths(new[] { resultJsonFilePath, resultFileName2 });
            Assert.NotNull(testResults);
            Assert.IsTrue(testResults.Count > 0);
            Assert.NotNull(performanceTestRunResults);
            Assert.IsTrue(performanceTestRunResults.Count > 0);
        }

        [Test]
        public void VerifyV2_AddPerformanceTestRunResults_OneResultFiles_OneResultDirectory()
        {
            // Arrange
            var resultJsonFilePath = EnsureFullPath("results2.Json");
            var resultsJsonDir = EnsureFullPath("ResultsJson2");
            var args = new[]
            {
                "--format=Json",
                    string.Format("--testresultsxmlsource={0}", resultJsonFilePath),
                    string.Format("--testresultsxmlsource={0}", resultsJsonDir)
                    , "--version=2"
            };
            optionsParser.ParseOptions(PerformanceBenchmark, args);

            // Act
            PerformanceBenchmark.AddPerformanceTestRunResults(testResultJsonParser, performanceTestRunResults, testResults, new List<TestResult>());

            // Assert
            Assert.IsTrue(PerformanceBenchmark.ResultFilesExist);
            AssertCorrectResultsJsonDirectoryPaths(new[] { resultsJsonDir });
            AssertCorrectResultJsonFilePaths(new[] { resultJsonFilePath });
            Assert.NotNull(testResults);
            Assert.IsTrue(testResults.Count > 0);
            Assert.NotNull(performanceTestRunResults);
            Assert.IsTrue(performanceTestRunResults.Count > 0);
        }

        [Test]
        public void VerifyV2_AddBaselinePerformanceTestRunResults()
        {
            // Arrange
            var resultJsonFilePath = EnsureFullPath("results2.Json");
            var baselineJsonFilePath = EnsureFullPath("baseline2.Json");
            var args = new[]
            {
                "--format=Json",
                string.Format("--testresultsxmlsource={0}", resultJsonFilePath),
                string.Format("--baselinexmlsource={0}", baselineJsonFilePath)
                , "--version=2"
            };
            optionsParser.ParseOptions(PerformanceBenchmark, args);

            // Act
            PerformanceBenchmark.AddBaselinePerformanceTestRunResults(testResultJsonParser, baselinePerformanceTestRunResults, baselineTestResults);

            // Assert
            Assert.IsTrue(PerformanceBenchmark.BaselineResultFilesExist);
            AssertCorrectBaselineJsonFilePaths(new[] { baselineJsonFilePath });
            AssertCorrectResultJsonFilePaths(new[] { resultJsonFilePath });
            Assert.NotNull(baselineTestResults);
            Assert.IsTrue(baselineTestResults.Count > 0);
            Assert.NotNull(baselinePerformanceTestRunResults);
            Assert.IsTrue(baselinePerformanceTestRunResults.Count > 0);
        }

        [Test]
        public void VerifyV2_AddBaselinePerformanceTestRunResultsDirectory()
        {
            // Arrange
            var resultJsonFilePath = EnsureFullPath("results2.Json");
            var baselineJsonFilePath = EnsureFullPath("baseline2.Json");
            var args = new[]
            {
                "--format=Json",
                string.Format("--testresultsxmlsource={0}", resultJsonFilePath),
                string.Format("--baselinexmlsource={0}", baselineJsonFilePath)
                , "--version=2"
            };
            optionsParser.ParseOptions(PerformanceBenchmark, args);

            // Act
            PerformanceBenchmark.AddBaselinePerformanceTestRunResults(testResultJsonParser, baselinePerformanceTestRunResults, baselineTestResults);

            // Assert
            Assert.IsTrue(PerformanceBenchmark.BaselineResultFilesExist);
            Assert.NotNull(baselineTestResults);
            Assert.IsTrue(baselineTestResults.Count > 0);
            Assert.NotNull(baselinePerformanceTestRunResults);
            Assert.IsTrue(baselinePerformanceTestRunResults.Count > 0);
        }

        [Test]
        public void VerifyV2_Verify_AddBaselineAndNonBaselinePerformanceTestRunResults()
        {
            // Arrange
            var resultJsonFilePath = EnsureFullPath("results2.Json");
            var baselineJsonFilePath = EnsureFullPath("baseline2.Json");
            var args = new[]
            {
                "--format=Json",
                    string.Format("--testresultsxmlsource={0}", resultJsonFilePath),
                    string.Format("--baselinexmlsource={0}", baselineJsonFilePath)
                    , "--version=2"
                };
            optionsParser.ParseOptions(PerformanceBenchmark, args);

            // Act
            PerformanceBenchmark.AddBaselinePerformanceTestRunResults(testResultJsonParser, baselinePerformanceTestRunResults, baselineTestResults);
            PerformanceBenchmark.AddPerformanceTestRunResults(testResultJsonParser, performanceTestRunResults, testResults, new List<TestResult>());

            // Assert
            Assert.IsTrue(PerformanceBenchmark.ResultFilesExist);
            AssertCorrectResultJsonFilePaths(new[] { resultJsonFilePath });
            Assert.NotNull(testResults);
            Assert.IsTrue(testResults.Count > 0);
            Assert.NotNull(performanceTestRunResults);
            Assert.IsTrue(performanceTestRunResults.Count > 0);

            Assert.IsTrue(PerformanceBenchmark.BaselineResultFilesExist);
            AssertCorrectBaselineJsonFilePaths(new[] { baselineJsonFilePath });
            Assert.NotNull(baselineTestResults);
            Assert.IsTrue(baselineTestResults.Count > 0);
            Assert.NotNull(baselinePerformanceTestRunResults);
            Assert.IsTrue(baselinePerformanceTestRunResults.Count > 0);
        }

        private void AssertCorrectBaselineJsonFilePaths(string[] baselineJsonFilePaths)
        {
            foreach (var baselineJsonFilePath in baselineJsonFilePaths)
            {
                Assert.IsFalse(PerformanceBenchmark.ResultFilePaths.Any(f => f.Equals(baselineJsonFilePath)));
                Assert.IsTrue(PerformanceBenchmark.BaselineFilePaths.Any(f => f.Equals(baselineJsonFilePath)));
            }
        }

        private void AssertCorrectResultJsonFilePaths(string[] resultFileNames)
        {
            foreach (var resultJsonFilePath in resultFileNames)
            {
                Assert.IsTrue(PerformanceBenchmark.ResultFilePaths.Contains(resultJsonFilePath));
                Assert.IsFalse(PerformanceBenchmark.BaselineFilePaths.Contains(resultJsonFilePath));
            }
        }

        private void AssertCorrectResultsJsonDirectoryPaths(string[] resultsJsonDirPaths)
        {
            foreach (var resultJsonDirPath in resultsJsonDirPaths)
            {
                Assert.IsTrue(PerformanceBenchmark.ResultDirectoryPaths.Contains(resultJsonDirPath));
            }
        }
    }
    
}
