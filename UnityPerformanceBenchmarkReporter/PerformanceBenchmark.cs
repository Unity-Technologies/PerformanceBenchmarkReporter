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
        public HashSet<string> ResultXmlFilePaths { get; } = new HashSet<string>();
        public HashSet<string> ResultXmlDirectoryPaths { get; } = new HashSet<string>();
        public HashSet<string> BaselineXmlFilePaths { get; } = new HashSet<string>();
        public HashSet<string> BaselineXmlDirectoryPaths { get; } = new HashSet<string>();
        public uint SigFig { get; private set; }
        public string ReportDirPath { get; set; }

        private bool firstResult = true;
        private string firstTestRunResultPath;
        private PerformanceTestRun firstTestRun = new PerformanceTestRun();
        private readonly PerformanceTestRunProcessor performanceTestRunProcessor = new PerformanceTestRunProcessor();
        private readonly string xmlFileExtension = ".xml";
        

        public bool BaselineResultFilesExist => BaselineXmlFilePaths.Any() || BaselineXmlDirectoryPaths.Any();

        public bool ResultFilesExist => ResultXmlFilePaths.Any() || ResultXmlDirectoryPaths.Any();

        public PerformanceBenchmark()
        {
            // Default significant figures to use for non-integer metrics if user doesn't specify another value.
            // Most values are in milliseconds or a count of something, so using more often creates an artificial baseline
            // failure based on insignificant digits
            SigFig = 0;
        }

        public void AddPerformanceTestRunResults(
            TestResultXmlParser testResultXmlParser, 
            List<PerformanceTestRunResult> performanceTestRunResults, 
            List<TestResult> testResults, 
            List<TestResult> baselineTestResults)
        {
            AddTestResults(testResultXmlParser, performanceTestRunResults, testResults, baselineTestResults, ResultXmlDirectoryPaths, ResultXmlFilePaths);
        }

        public void AddBaselinePerformanceTestRunResults(
            TestResultXmlParser testResultXmlParser, 
            List<PerformanceTestRunResult> baselinePerformanceTestRunResults, 
            List<TestResult> baselineTestResults)
        {
            AddTestResults(testResultXmlParser, baselinePerformanceTestRunResults, baselineTestResults, baselineTestResults, BaselineXmlDirectoryPaths, BaselineXmlFilePaths, true);
        }

        public void AddTestResults(
            TestResultXmlParser testResultXmlParser,  
            List<PerformanceTestRunResult> runResults, 
            List<TestResult> testResults, 
            List<TestResult> baselineTestResults,
            HashSet<string> xmlDirectoryPaths, 
            HashSet<string> xmlFileNamePaths,
            bool isBaseline = false)
        {
            if (xmlDirectoryPaths.Any())
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
                foreach (var xmlFileNamePath in xmlFileNamePaths)
                {
                    var performanceTestRun = testResultXmlParser.GetPerformanceTestRunFromXml(xmlFileNamePath);

                    if (performanceTestRun != null && performanceTestRun.Results.Any())
                    {
                        var results = performanceTestRunProcessor.GetTestResults(performanceTestRun);
                        if (!results.Any())
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("No performance test data found to report in: {0}", xmlFileNamePath);
                            Console.ResetColor();
                        }
                        testResults.AddRange(results);

                        performanceTestRunProcessor.UpdateTestResultsBasedOnBaselineResults(baselineTestResults, testResults, SigFig);

                        ValidateMetadata(performanceTestRun, xmlFileNamePath);
                        runResults.Add(performanceTestRunProcessor.CreateTestRunResult
                            (
                                firstTestRun,
                                testResults,
                                Path.GetFileNameWithoutExtension(xmlFileNamePath),
                                isBaseline)
                        );
                    }
                }
            }
        }

        private IEnumerable<string> GetAllXmlFileNames(string xmlDirectory)
        {
            var dir = new DirectoryInfo(xmlDirectory);
            var xmlFileNames = dir.GetFiles("*" + xmlFileExtension, SearchOption.AllDirectories).Select(f => f.FullName);
            return xmlFileNames;
        }

        private void ValidateMetadata(PerformanceTestRun performanceTestRun, string xmlFileNamePath)
        {
            if (firstResult)
            {
                firstTestRunResultPath = xmlFileNamePath;
                firstTestRun = performanceTestRun;
                firstResult = false;
            }
            else
            {
                var metadataValidator = new MetadataValidator();
                metadataValidator.ValidatePlayerSystemInfo(firstTestRun, performanceTestRun, firstTestRunResultPath, xmlFileNamePath);
                metadataValidator.ValidatePlayerSettings(firstTestRun, performanceTestRun, firstTestRunResultPath, xmlFileNamePath);
                metadataValidator.ValidateQualitySettings(firstTestRun, performanceTestRun, firstTestRunResultPath, xmlFileNamePath);
                metadataValidator.ValidateScreenSettings(firstTestRun, performanceTestRun, firstTestRunResultPath, xmlFileNamePath);
            }
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
                throw new ArgumentException(string.Format("{0} directory `{1}` doesn't contain any .xml files.", optionName,
                    xmlSourcePath));
            }

            switch (resultType)
            {
                case OptionsParser.ResultType.Test:
                    ResultXmlDirectoryPaths.Add(xmlSourcePath);
                    break;
                case OptionsParser.ResultType.Baseline:
                    BaselineXmlDirectoryPaths.Add(xmlSourcePath);
                    break;
                default:
                    throw new InvalidEnumArgumentException(resultType.ToString());
            }
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

        public void AddSigFig(uint sigFig)
        {
            SigFig = sigFig;
        }
    }
}
