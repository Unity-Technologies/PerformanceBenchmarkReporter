using System;
using System.Collections.Generic;
using System.Linq;
using UnityPerformanceBenchmarkReporter.Entities;
using UnityPerformanceBenchmarkReporter.Report;

namespace UnityPerformanceBenchmarkReporter
{
    internal class Program
    {
        private static readonly Dictionary<Type, string[]> ExcludedConfigFieldNames = new Dictionary<Type, string[]>
        {
            {typeof(EditorVersion), new []
            {
                "DateSeconds", 
                "RevisionValue", 
                "Branch"
            }},
            {typeof(PlayerSettings), new []
            {
                "MtRendering", // Hidden because we have a calculated field, RenderThreadingMode, that provides a more succinct value (SingleThreaded, MultiThreaded, GfxJobs)
                "GraphicsJobs", // Hidden because we have a calculated field, RenderThreadingMode, that provides a more succinct value (SingleThreaded, MultiThreaded, GfxJobs)
                "VrSupported" // Hidden because this value doesn't seem to be coming through as 'True' when it should be true.
            }}
        };

        private static void Main(string[] args)
        {
            var aggregateTestRunResults = new List<PerformanceTestRunResult>();
            var baselinePerformanceTestRunResults = new List<PerformanceTestRunResult>();
            var baselineTestResults = new List<TestResult>();
            var performanceTestRunResults = new List<PerformanceTestRunResult>();
            var testResults = new List<TestResult>();
            var performanceBenchmark = new PerformanceBenchmark(ExcludedConfigFieldNames);
            var optionsParser = new OptionsParser();

            optionsParser.ParseOptions(performanceBenchmark, args);
            var testResultXmlParser = new TestResultXmlParser();

            if (performanceBenchmark.BaselineResultFilesExist)
            {
                performanceBenchmark.AddBaselinePerformanceTestRunResults(testResultXmlParser, baselinePerformanceTestRunResults, baselineTestResults);

                if (baselinePerformanceTestRunResults.Any())
                {
                    aggregateTestRunResults.AddRange(baselinePerformanceTestRunResults);
                }
                else
                {
                    Environment.Exit(1);
                }
            }

            if (performanceBenchmark.ResultFilesExist)
            {
                performanceBenchmark.AddPerformanceTestRunResults(testResultXmlParser, performanceTestRunResults, testResults, baselineTestResults);

                if (performanceTestRunResults.Any())
                {
                    aggregateTestRunResults.AddRange(performanceTestRunResults);
                }
                else
                {
                    Environment.Exit(1);
                }
            }

            var performanceTestResults = new PerformanceTestRunResult[0]; 

            if (aggregateTestRunResults.Any(a => a.IsBaseline))
            {
                Array.Resize(ref performanceTestResults, 1);
                performanceTestResults[0] = aggregateTestRunResults.First(a => a.IsBaseline);
            }

            var nonBaselineTestRunResults = aggregateTestRunResults.Where(a => !a.IsBaseline).ToList();

            nonBaselineTestRunResults.Sort((run1, run2) => string.Compare(run1.ResultName, run2.ResultName, StringComparison.Ordinal));


            foreach (var performanceTestRunResult in nonBaselineTestRunResults)
            {
                Array.Resize(ref performanceTestResults, performanceTestResults.Length + 1);
                performanceTestResults[performanceTestResults.Length - 1] = performanceTestRunResult;
            }


            var reportWriter = new ReportWriter(performanceBenchmark.TestRunMetadataProcessor);
            reportWriter.WriteReport(
                performanceTestResults, 
                performanceBenchmark.SigFig, 
                performanceBenchmark.ReportDirPath, 
                performanceBenchmark.BaselineResultFilesExist);
        }
    }
}
