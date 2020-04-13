using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using UnityPerformanceBenchmarkReporter.Entities;

namespace UnityPerformanceBenchmarkReporter
{
    public class PerformanceBenchmark
    {
        private readonly PerformanceTestRunProcessor performanceTestRunProcessor = new PerformanceTestRunProcessor();

        public readonly TestRunMetadataProcessor TestRunMetadataProcessor;
        private readonly string xmlFileExtension = ".xml";

        public PerformanceBenchmark(Dictionary<Type, string[]> configFieldNames = null)
        {
            // Default significant figures to use for non-integer metrics if user doesn't specify another value.
            // Most values are in milliseconds or a count of something, so using more often creates an artificial baseline
            // failure based on insignificant digits equating to a microsecond, or less, time difference. The Unity Profiler only shows
            // up to 2 significant figures for milliseconds as well, so this is what folks are used to working with.
            SigFig = 2;
            TestRunMetadataProcessor = new TestRunMetadataProcessor(configFieldNames);
        }

        public HashSet<string> ResultXmlFilePaths { get; } = new HashSet<string>();
        public HashSet<string> ResultXmlDirectoryPaths { get; } = new HashSet<string>();
        public HashSet<string> BaselineXmlFilePaths { get; } = new HashSet<string>();
        public uint SigFig { get; }
        public string ReportDirPath { get; private set; }
        public bool FailOnBaseline { get; set; }


        public bool BaselineResultFilesExist => BaselineXmlFilePaths.Any();

        public bool ResultFilesExist => ResultXmlFilePaths.Any() || ResultXmlDirectoryPaths.Any();

        public void AddPerformanceTestRunResults(
            TestResultXmlParser testResultXmlParser,
            List<PerformanceTestRunResult> performanceTestRunResults,
            List<TestResult> testResults,
            List<TestResult> baselineTestResults)
        {
            AddTestResults(testResultXmlParser, performanceTestRunResults, testResults, baselineTestResults,
                ResultXmlDirectoryPaths, ResultXmlFilePaths);
        }

        public void AddBaselinePerformanceTestRunResults(
            TestResultXmlParser testResultXmlParser,
            List<PerformanceTestRunResult> baselinePerformanceTestRunResults,
            List<TestResult> baselineTestResults)
        {
            AddTestResults(testResultXmlParser, baselinePerformanceTestRunResults, baselineTestResults,
                baselineTestResults, null, BaselineXmlFilePaths, true);
        }

        private void AddTestResults(
            TestResultXmlParser testResultXmlParser,
            List<PerformanceTestRunResult> testRunResults,
            List<TestResult> testResults,
            List<TestResult> baselineTestResults,
            HashSet<string> xmlDirectoryPaths,
            HashSet<string> xmlFileNamePaths,
            bool isBaseline = false)
        {
            if (!isBaseline && xmlDirectoryPaths != null && xmlDirectoryPaths.Any())
            {
                foreach (var xmlDirectory in xmlDirectoryPaths)
                {
                    var xmlFileNames = GetAllXmlFileNames(xmlDirectory);

                    foreach (var xmlFileName in xmlFileNames)
                    {
                        xmlFileNamePaths.Add(xmlFileName);
                    }
                }
            }

            if (xmlFileNamePaths.Any())
            {
                var perfTestRuns = new List<KeyValuePair<string, PerformanceTestRun>>();

                foreach (var xmlFileNamePath in xmlFileNamePaths)
                {
                    var performanceTestRun = testResultXmlParser.Parse(xmlFileNamePath);
                    if (performanceTestRun != null && performanceTestRun.Results.Any())
                    {
                        perfTestRuns.Add(
                            new KeyValuePair<string, PerformanceTestRun>(xmlFileNamePath, performanceTestRun));
                    }
                }

                perfTestRuns.Sort((run1, run2) => string.Compare(run1.Key, run2.Key, StringComparison.Ordinal));
                var resultFilesOrderedByResultName = perfTestRuns.ToArray();

                for (var i = 0; i < resultFilesOrderedByResultName.Length; i++)
                {
                    var performanceTestRun =
                        testResultXmlParser.Parse(resultFilesOrderedByResultName[i].Key);

                    if (performanceTestRun != null && performanceTestRun.Results.Any())
                    {
                        var results = performanceTestRunProcessor.GetTestResults(performanceTestRun);
                        if (!results.Any())
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("No performance test data found to report in: {0}",
                                resultFilesOrderedByResultName[i].Key);
                            Console.ResetColor();
                            continue;
                        }

                        testResults.AddRange(results);

                        performanceTestRunProcessor.UpdateTestResultsBasedOnBaselineResults(baselineTestResults, testResults, SigFig);

                        TestRunMetadataProcessor.ProcessMetadata(performanceTestRun, resultFilesOrderedByResultName[i].Key);

                        var performanceTestRunResult = performanceTestRunProcessor.CreateTestRunResult(
                                                                performanceTestRun,
                                                                results,
                                                                Path.GetFileNameWithoutExtension(resultFilesOrderedByResultName[i].Key),
                                                                isBaseline);
                        testRunResults.Add(performanceTestRunResult);
                    }
                }
            }
        }

        private IEnumerable<string> GetAllXmlFileNames(string xmlDirectory)
        {
            var dir = new DirectoryInfo(xmlDirectory);
            var xmlFileNames = dir.GetFiles("*" + xmlFileExtension, SearchOption.AllDirectories)
                .Select(f => f.FullName);
            return xmlFileNames;
        }

        public void AddXmlSourcePath(string xmlSourcePath, string optionName, OptionsParser.ResultType resultType)
        {
            if (string.IsNullOrEmpty(xmlSourcePath))
            {
                throw new ArgumentNullException(xmlSourcePath);
            }

            if (string.IsNullOrEmpty(optionName))
            {
                throw new ArgumentNullException(optionName);
            }

            //If has .xml file extension
            if (xmlSourcePath.EndsWith(xmlFileExtension))
            {
                ProcessAsXmlFile(xmlSourcePath, optionName, resultType);
            }
            else
            {
                ProcessAsXmlDirectory(xmlSourcePath, optionName, resultType);
            }
        }

        private void ProcessAsXmlDirectory(string xmlSourcePath, string optionName, OptionsParser.ResultType resultType)
        {
            if (!Directory.Exists(xmlSourcePath))
            {
                throw new ArgumentException(string.Format("{0} directory `{1}` cannot be found", optionName,
                    xmlSourcePath));
            }

            var xmlFileNames = GetAllXmlFileNames(xmlSourcePath).ToArray();
            if (!xmlFileNames.Any())
            {
                throw new ArgumentException(string.Format("{0} directory `{1}` doesn't contain any .xml files.",
                    optionName,
                    xmlSourcePath));
            }

            ResultXmlDirectoryPaths.Add(xmlSourcePath);
        }

        private void ProcessAsXmlFile(string xmlSourcePath, string optionName, OptionsParser.ResultType resultType)
        {
            if (!File.Exists(xmlSourcePath))
            {
                throw new ArgumentException(string.Format("{0} file `{1}` cannot be found", optionName, xmlSourcePath));
            }

            switch (resultType)
            {
                case OptionsParser.ResultType.Test:
                    ResultXmlFilePaths.Add(xmlSourcePath);
                    break;
                case OptionsParser.ResultType.Baseline:
                    BaselineXmlFilePaths.Add(xmlSourcePath);
                    break;
                default:
                    throw new InvalidEnumArgumentException(resultType.ToString());
            }
        }

        public void AddReportDirPath(string reportDirectoryPath)
        {
            ReportDirPath = reportDirectoryPath;
        }
    }
}