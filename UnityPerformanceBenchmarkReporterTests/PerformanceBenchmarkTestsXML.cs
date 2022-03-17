using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityPerformanceBenchmarkReporter;
using UnityPerformanceBenchmarkReporter.Entities;

namespace UnityPerformanceBenchmarkReporterTests
{
    public class PerformanceBenchmarkTestsXML : PerformanceBenchmarkTestsBase
    {
        private OptionsParser optionsParser;
        private IParser testResultXmlParser;
        private List<PerformanceTestRunResult> performanceTestRunResults;
        private List<TestResult> testResults;
        private List<PerformanceTestRunResult> baselinePerformanceTestRunResults;
        private List<TestResult> baselineTestResults;

        [SetUp]
        public void Setup()
        {
            optionsParser = new OptionsParser();
            PerformanceBenchmark = new PerformanceBenchmark();
            testResultXmlParser = new TestResultXmlParser();
            performanceTestRunResults = new List<PerformanceTestRunResult>();
            testResults = new List<TestResult>();
            baselinePerformanceTestRunResults = new List<PerformanceTestRunResult>();
            baselineTestResults = new List<TestResult>();
        }


        [Test]
        public void Verify_AddPerformanceTestRunResults()
        {
            // Arrange
            var resultXmlFilePath = EnsureFullPath("results.xml");
            var args = new[] { string.Format("--testresultsxmlsource={0}", resultXmlFilePath) };
            optionsParser.ParseOptions(PerformanceBenchmark, args);

            // Act
            PerformanceBenchmark.AddPerformanceTestRunResults(testResultXmlParser, performanceTestRunResults, testResults, new List<TestResult>());

            // Assert
            Assert.IsTrue(PerformanceBenchmark.ResultFilesExist);
            AssertCorrectResultXmlFilePaths(new[] { resultXmlFilePath });
            Assert.NotNull(testResults);
            Assert.IsTrue(testResults.Count > 0);
            Assert.NotNull(performanceTestRunResults);
            Assert.IsTrue(performanceTestRunResults.Count > 0);
        }

        [Test]
        public void Verify_AddPerformanceTestRunResults_TwoResultFiles()
        {
            // Arrange
            var resultXmlFilePath = EnsureFullPath("results.xml");
            var resultFileName2 = EnsureFullPath("results2.xml");
            var args = new[]
            {
                string.Format("--testresultsxmlsource={0}", resultXmlFilePath),
                string.Format("--testresultsxmlsource={0}", resultFileName2)
            };
            optionsParser.ParseOptions(PerformanceBenchmark, args);

            // Act
            PerformanceBenchmark.AddPerformanceTestRunResults(testResultXmlParser, performanceTestRunResults, testResults, new List<TestResult>());

            // Assert
            Assert.IsTrue(PerformanceBenchmark.ResultFilesExist);
            AssertCorrectResultXmlFilePaths(new[] { resultXmlFilePath, resultFileName2 });
            Assert.NotNull(testResults);
            Assert.IsTrue(testResults.Count > 0);
            Assert.NotNull(performanceTestRunResults);
            Assert.IsTrue(performanceTestRunResults.Count > 0);
        }

        [Test]
        public void Verify_AddPerformanceTestRunResults_OneResultFiles_OneResultDirectory()
        {
            // Arrange
            var resultXmlFilePath = EnsureFullPath("results.xml");
            var resultsXmlDir = EnsureFullPath("Results");
            var args = new[]
            {
                    string.Format("--testresultsxmlsource={0}", resultXmlFilePath),
                    string.Format("--testresultsxmlsource={0}", resultsXmlDir)
            };
            optionsParser.ParseOptions(PerformanceBenchmark, args);

            // Act
            PerformanceBenchmark.AddPerformanceTestRunResults(testResultXmlParser, performanceTestRunResults, testResults, new List<TestResult>());

            // Assert
            Assert.IsTrue(PerformanceBenchmark.ResultFilesExist);
            AssertCorrectResultsXmlDirectoryPaths(new[] { resultsXmlDir });
            AssertCorrectResultXmlFilePaths(new[] { resultXmlFilePath });
            Assert.NotNull(testResults);
            Assert.IsTrue(testResults.Count > 0);
            Assert.NotNull(performanceTestRunResults);
            Assert.IsTrue(performanceTestRunResults.Count > 0);
        }

        [Test]
        public void Verify_AddBaselinePerformanceTestRunResults()
        {
            // Arrange
            var resultXmlFilePath = EnsureFullPath("results.xml");
            var baselineXmlFilePath = EnsureFullPath("baseline.xml");
            var args = new[]
            {
                string.Format("--testresultsxmlsource={0}", resultXmlFilePath),
                string.Format("--baselinexmlsource={0}", baselineXmlFilePath)
            };
            optionsParser.ParseOptions(PerformanceBenchmark, args);

            // Act
            PerformanceBenchmark.AddBaselinePerformanceTestRunResults(testResultXmlParser, baselinePerformanceTestRunResults, baselineTestResults);

            // Assert
            Assert.IsTrue(PerformanceBenchmark.BaselineResultFilesExist);
            AssertCorrectBaselineXmlFilePaths(new[] { baselineXmlFilePath });
            AssertCorrectResultXmlFilePaths(new[] { resultXmlFilePath });
            Assert.NotNull(baselineTestResults);
            Assert.IsTrue(baselineTestResults.Count > 0);
            Assert.NotNull(baselinePerformanceTestRunResults);
            Assert.IsTrue(baselinePerformanceTestRunResults.Count > 0);
        }

        [Test]
        public void Verify_AddBaselinePerformanceTestRunResultsDirectory()
        {
            // Arrange
            var resultXmlFilePath = EnsureFullPath("results.xml");
            var baselineXmlFilePath = EnsureFullPath("Baselines");
            var args = new[]
            {
                string.Format("--testresultsxmlsource={0}", resultXmlFilePath),
                string.Format("--baselinexmlsource={0}", baselineXmlFilePath)
            };
            optionsParser.ParseOptions(PerformanceBenchmark, args);

            // Act
            PerformanceBenchmark.AddBaselinePerformanceTestRunResults(testResultXmlParser, baselinePerformanceTestRunResults, baselineTestResults);

            // Assert
            Assert.IsTrue(PerformanceBenchmark.BaselineResultFilesExist);
            Assert.NotNull(baselineTestResults);
            Assert.IsTrue(baselineTestResults.Count > 0);
            Assert.NotNull(baselinePerformanceTestRunResults);
            Assert.IsTrue(baselinePerformanceTestRunResults.Count > 0);
        }

        [Test]
        public void Verify_Verify_AddBaselineAndNonBaselinePerformanceTestRunResults()
        {
            // Arrange
            var resultXmlFilePath = EnsureFullPath("results.xml");
            var baselineXmlFilePath = EnsureFullPath("baseline.xml");
            var args = new[]
            {
                    string.Format("--testresultsxmlsource={0}", resultXmlFilePath),
                    string.Format("--baselinexmlsource={0}", baselineXmlFilePath)
                };
            optionsParser.ParseOptions(PerformanceBenchmark, args);

            // Act
            PerformanceBenchmark.AddBaselinePerformanceTestRunResults(testResultXmlParser, baselinePerformanceTestRunResults, baselineTestResults);
            PerformanceBenchmark.AddPerformanceTestRunResults(testResultXmlParser, performanceTestRunResults, testResults, new List<TestResult>());

            // Assert
            Assert.IsTrue(PerformanceBenchmark.ResultFilesExist);
            AssertCorrectResultXmlFilePaths(new[] { resultXmlFilePath });
            Assert.NotNull(testResults);
            Assert.IsTrue(testResults.Count > 0);
            Assert.NotNull(performanceTestRunResults);
            Assert.IsTrue(performanceTestRunResults.Count > 0);

            Assert.IsTrue(PerformanceBenchmark.BaselineResultFilesExist);
            AssertCorrectBaselineXmlFilePaths(new[] { baselineXmlFilePath });
            Assert.NotNull(baselineTestResults);
            Assert.IsTrue(baselineTestResults.Count > 0);
            Assert.NotNull(baselinePerformanceTestRunResults);
            Assert.IsTrue(baselinePerformanceTestRunResults.Count > 0);
        }

        private void AssertCorrectBaselineXmlFilePaths(string[] baselineXmlFilePaths)
        {
            foreach (var baselineXmlFilePath in baselineXmlFilePaths)
            {
                Assert.IsFalse(PerformanceBenchmark.ResultFilePaths.Any(f => f.Equals(baselineXmlFilePath)));
                Assert.IsTrue(PerformanceBenchmark.BaselineFilePaths.Any(f => f.Equals(baselineXmlFilePath)));
            }
        }

        private void AssertCorrectResultXmlFilePaths(string[] resultFileNames)
        {
            foreach (var resultXmlFilePath in resultFileNames)
            {
                Assert.IsTrue(PerformanceBenchmark.ResultFilePaths.Contains(resultXmlFilePath));
                Assert.IsFalse(PerformanceBenchmark.BaselineFilePaths.Contains(resultXmlFilePath));
            }
        }

        private void AssertCorrectResultsXmlDirectoryPaths(string[] resultsXmlDirPaths)
        {
            foreach (var resultXmlDirPath in resultsXmlDirPaths)
            {
                Assert.IsTrue(PerformanceBenchmark.ResultDirectoryPaths.Contains(resultXmlDirPath));
            }
        }
    }
}
