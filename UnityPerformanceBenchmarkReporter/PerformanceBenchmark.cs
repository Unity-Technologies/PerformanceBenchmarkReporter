﻿using System;
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
        public string ReportDirPath { get; private set; }

        public readonly TestRunMetadataProcessor TestRunMetadataProcessor;

        private bool firstResult = true;
        private string firstTestRunResultPath;
        private PerformanceTestRun firstTestRun = new PerformanceTestRun();
        private readonly PerformanceTestRunProcessor performanceTestRunProcessor = new PerformanceTestRunProcessor();
        private readonly string xmlFileExtension = ".xml";
        private readonly Dictionary<Type, string[]> excludedConfigFieldNames = new Dictionary<Type, string[]>();


        public bool BaselineResultFilesExist => BaselineXmlFilePaths.Any() || BaselineXmlDirectoryPaths.Any();

        public bool ResultFilesExist => ResultXmlFilePaths.Any() || ResultXmlDirectoryPaths.Any();

        public PerformanceBenchmark(Dictionary<Type, string[]> configFieldNames = null)
        {
            // Default significant figures to use for non-integer metrics if user doesn't specify another value.
            // Most values are in milliseconds or a count of something, so using more often creates an artificial baseline
            // failure based on insignificant digits equating to a microsecond, or less, time difference. The Unity Profiler only shows
            // up to 2 significant figures for milliseconds as well, so this is what folks are used to working with.
            SigFig = 2;

            if (configFieldNames != null)
            {
                excludedConfigFieldNames = configFieldNames;
            }

            TestRunMetadataProcessor = new TestRunMetadataProcessor(configFieldNames);
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

        private void AddTestResults(
            TestResultXmlParser testResultXmlParser,  
            List<PerformanceTestRunResult> testRunResults, 
            List<TestResult> testResults, 
            List<TestResult> baselineTestResults,
            HashSet<string> xmlDirectoryPaths, 
            HashSet<string> xmlFileNamePaths,
            bool isBaseline = false)
        {
            if (!isBaseline && xmlDirectoryPaths.Any())
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
                    var performanceTestRun = testResultXmlParser.GetPerformanceTestRunFromXml(xmlFileNamePath);
                    if (performanceTestRun != null && performanceTestRun.Results.Any())
                    {
                        perfTestRuns.Add( new KeyValuePair<string, PerformanceTestRun>(xmlFileNamePath, performanceTestRun));
                    }
                }

                perfTestRuns.Sort((run1, run2) => run1.Value.StartTime.CompareTo(run2.Value.StartTime));
                var resultFilesOrderByStartTime = perfTestRuns.ToArray();

                for (var i = 0; i < resultFilesOrderByStartTime.Length; i++)
                {
                    var performanceTestRun = testResultXmlParser.GetPerformanceTestRunFromXml(resultFilesOrderByStartTime[i].Key);

                    if (performanceTestRun != null && performanceTestRun.Results.Any())
                    {
                        var results = performanceTestRunProcessor.GetTestResults(performanceTestRun);
                        if (!results.Any())
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("No performance test data found to report in: {0}", resultFilesOrderByStartTime[i].Key);
                            Console.ResetColor();
                            continue;
                        }
                        testResults.AddRange(results);

                        performanceTestRunProcessor.UpdateTestResultsBasedOnBaselineResults(baselineTestResults, testResults, SigFig);

                        TestRunMetadataProcessor.ProcessMetadata(performanceTestRun, resultFilesOrderByStartTime[i].Key);
                       
                        testRunResults.Add(performanceTestRunProcessor.CreateTestRunResult
                            (
                                performanceTestRun,
                                results,
                                Path.GetFileNameWithoutExtension(resultFilesOrderByStartTime[i].Key),
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
        
        //private void ProcessMetadata(PerformanceTestRun performanceTestRun, string xmlFileNamePath)
        //{
        //    //TODO add check using TestRunMetadataExists()
        //    //if()
        //    TestRunMetadataProcessor.SetIsVrSupported(new[] { performanceTestRun });
        //    TestRunMetadataProcessor.SetIsAndroid(new[] { performanceTestRun });

        //    if (firstResult)
        //    {
        //        firstTestRunResultPath = xmlFileNamePath;
        //        firstTestRun = performanceTestRun;
        //        firstResult = false;
        //    }
        //    else
        //    {
        //        TestRunMetadataProcessor.ValidatePlayerSystemInfo(firstTestRun, performanceTestRun, firstTestRunResultPath, xmlFileNamePath, ExcludedFieldNames<PlayerSystemInfo>());
        //        TestRunMetadataProcessor.ValidatePlayerSettings(firstTestRun, performanceTestRun, firstTestRunResultPath, xmlFileNamePath, ExcludedFieldNames<PlayerSettings>());
        //        TestRunMetadataProcessor.ValidateQualitySettings(firstTestRun, performanceTestRun, firstTestRunResultPath, xmlFileNamePath, ExcludedFieldNames<QualitySettings>());
        //        TestRunMetadataProcessor.ValidateScreenSettings(firstTestRun, performanceTestRun, firstTestRunResultPath, xmlFileNamePath, ExcludedFieldNames<ScreenSettings>());
        //        TestRunMetadataProcessor.ValidateBuildSettings(firstTestRun, performanceTestRun, firstTestRunResultPath, xmlFileNamePath, ExcludedFieldNames<BuildSettings>());
        //        TestRunMetadataProcessor.ValidateEditorVersion(firstTestRun, performanceTestRun, firstTestRunResultPath, xmlFileNamePath, ExcludedFieldNames<EditorVersion>());
        //    }
        //}

        private string[] ExcludedFieldNames<T>()
        {
            return excludedConfigFieldNames.ContainsKey(typeof(T))
                ? excludedConfigFieldNames[typeof(T)]
                : null;
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
