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
        private ESupportedFileTypes fileExtension = ESupportedFileTypes.xml;
        public ESupportedFileTypes FileType { get { return fileExtension; } }

        public int DataVersion { get; private set; } = 2;
        public PerformanceBenchmark(Dictionary<Type, string[]> configFieldNames = null)
        {
            // Default significant figures to use for non-integer metrics if user doesn't specify another value.
            // Most values are in milliseconds or a count of something, so using more often creates an artificial baseline
            // failure based on insignificant digits equating to a microsecond, or less, time difference. The Unity Profiler only shows
            // up to 2 significant figures for milliseconds as well, so this is what folks are used to working with.
            SigFig = 2;
            TestRunMetadataProcessor = new TestRunMetadataProcessor(configFieldNames);
        }

        public HashSet<string> ResultFilePaths { get; } = new HashSet<string>();
        public HashSet<string> ResultDirectoryPaths { get; } = new HashSet<string>();
        public HashSet<string> BaselineFilePaths { get; } = new HashSet<string>();
        public uint SigFig { get; }
        public string ReportDirPath { get; private set; }
        public bool FailOnBaseline { get; set; }


        public bool BaselineResultFilesExist => BaselineFilePaths.Any();

        public bool ResultFilesExist => ResultFilePaths.Any() || ResultDirectoryPaths.Any();

        public List<string> IgnoredMetrics = new List<string>();


        public void AddPerformanceTestRunResults(
            IParser testResultParser,
            List<PerformanceTestRunResult> performanceTestRunResults,
            List<TestResult> testResults,
            List<TestResult> baselineTestResults)
        {
            AddTestResults(testResultParser, performanceTestRunResults, testResults, baselineTestResults,
                ResultDirectoryPaths, ResultFilePaths);
        }

        public void AddBaselinePerformanceTestRunResults(
            IParser testResultParser,
            List<PerformanceTestRunResult> baselinePerformanceTestRunResults,
            List<TestResult> baselineTestResults)
        {
            AddTestResults(testResultParser, baselinePerformanceTestRunResults, baselineTestResults,
                baselineTestResults, null, BaselineFilePaths, true);
        }

        private void AddTestResults(
            IParser testResultParser,
            List<PerformanceTestRunResult> testRunResults,
            List<TestResult> testResults,
            List<TestResult> baselineTestResults,
            HashSet<string> directoryPaths,
            HashSet<string> fileNamePaths,
            bool isBaseline = false)
        {
            if (!isBaseline && directoryPaths != null && directoryPaths.Any())
            {
                foreach (var directory in directoryPaths)
                {
                    var fileNames = GetAllFileNames(directory);

                    foreach (var fileName in fileNames)
                    {
                        fileNamePaths.Add(fileName);
                    }
                }
            }

            if (fileNamePaths.Any())
            {
                var perfTestRuns = new List<KeyValuePair<string, PerformanceTestRun>>();

                foreach (var fileNamePath in fileNamePaths)
                {
                    var performanceTestRun = testResultParser.Parse(fileNamePath, DataVersion);
                    if (performanceTestRun != null && performanceTestRun.Results.Any())
                    {
                        perfTestRuns.Add(
                            new KeyValuePair<string, PerformanceTestRun>(fileNamePath, performanceTestRun));
                    }
                }

                perfTestRuns.Sort((run1, run2) => string.Compare(run1.Key, run2.Key, StringComparison.Ordinal));
                var resultFilesOrderedByResultName = perfTestRuns.ToArray();

                for (var i = 0; i < resultFilesOrderedByResultName.Length; i++)
                {
                    var performanceTestRun =
                        testResultParser.Parse(resultFilesOrderedByResultName[i].Key, DataVersion);

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

                        performanceTestRunProcessor.UpdateTestResultsBasedOnBaselineResults(baselineTestResults, IgnoredMetrics, testResults, SigFig);

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

        public void SetIgnoredMetrics(string metrics)
        {
            IgnoredMetrics = metrics.Split(';').ToList();
        }

        public void SetDataVersion(string version)
        {
            if (int.TryParse(version, out int result))
            {
                if (result > 0 && result < 3)
                    DataVersion = result;
                else
                    throw new ArgumentException($"{version} is not a valid data format version. Please pass 1 or 2");
            }
            else
            {
                throw new ArgumentException($"{version} is not a valid data format version");
            }
        }

        public void SetFileType(string filetype)
        {
            if (String.IsNullOrEmpty(filetype))
                return;

            if (Enum.TryParse<ESupportedFileTypes>(filetype, true, out ESupportedFileTypes result))
            {
                fileExtension = result;
            }
            else
            {
                throw new ArgumentException($"{filetype} is not a valid file format");
            }
        }

        private IEnumerable<string> GetAllFileNames(string directory)
        {
            var dir = new DirectoryInfo(directory);
            var FileNames = dir.GetFiles("*" + fileExtension, SearchOption.AllDirectories)
                .Select(f => f.FullName);
            return FileNames;
        }

        public void AddSourcePath(string sourcePath, string optionName, OptionsParser.ResultType resultType)
        {
            System.Console.WriteLine($" Adding Source Path : {sourcePath}");
            System.Console.WriteLine($"");

            if (string.IsNullOrEmpty(sourcePath))
            {
                throw new ArgumentNullException(sourcePath);
            }

            if (string.IsNullOrEmpty(optionName))
            {
                throw new ArgumentNullException(optionName);
            }

            if (sourcePath.EndsWith(fileExtension.ToString()))
            {
                ProcessAsFile(sourcePath, optionName, resultType);
            }
            else
            {
                ProcessAsDirectory(sourcePath, optionName, resultType);
            }




        }

        private void ProcessAsDirectory(string sourcePath, string optionName, OptionsParser.ResultType resultType)
        {
            if (!Directory.Exists(sourcePath))
            {
                throw new ArgumentException(string.Format("{0} directory `{1}` cannot be found", optionName,
                    sourcePath));
            }

            var fileNames = GetAllFileNames(sourcePath).ToArray();
            if (!fileNames.Any())
            {
                throw new ArgumentException(string.Format("{0} directory `{1}` doesn't contain any {2} files.",
                    optionName,
                    sourcePath, FileType));
            }

            switch (resultType)
            {
                case OptionsParser.ResultType.Test:
                    ResultDirectoryPaths.Add(sourcePath);
                    break;
                case OptionsParser.ResultType.Baseline:
                    foreach (var filename in fileNames)
                    {
                        BaselineFilePaths.Add(filename);
                    }

                    break;
                default:
                    throw new InvalidEnumArgumentException(resultType.ToString());
            }
        }

        private void ProcessAsFile(string sourcePath, string optionName, OptionsParser.ResultType resultType)
        {
            if (!File.Exists(sourcePath))
            {
                throw new ArgumentException(string.Format("{0} file `{1}` cannot be found", optionName, sourcePath));
            }

            switch (resultType)
            {
                case OptionsParser.ResultType.Test:
                    ResultFilePaths.Add(sourcePath);
                    break;
                case OptionsParser.ResultType.Baseline:
                    BaselineFilePaths.Add(sourcePath);
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