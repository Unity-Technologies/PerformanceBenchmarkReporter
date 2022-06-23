using System;
using System.Collections.Generic;
using System.Linq;
using UnityPerformanceBenchmarkReporter.Entities;

namespace UnityPerformanceBenchmarkReporter
{
    internal enum MeasurementResult
    {
        Neutral = 0,
        Regression = 1,
        Progression = 2,
        RegressionKnown = 3
    }

    public class PerformanceTestRunProcessor
    {
        public List<TestResult> GetTestResults(
            PerformanceTestRun performanceTestRun)
        {
            var mergedTestExecutions = MergeTestExecutions(performanceTestRun);
            var performanceTestResults = new List<TestResult>();
            foreach (var testName in mergedTestExecutions.Keys)
            {
                var performanceTestResult = new TestResult
                {
                    TestName = testName,
                    TestCategories = performanceTestRun.Results.First(r => r.TestName == testName).TestCategories,
                    TestVersion = performanceTestRun.Results.First(r => r.TestName == testName).TestVersion,
                    State = (int)TestState.Success,
                    SampleGroupResults = new List<SampleGroupResult>()
                };
                foreach (var sampleGroup in mergedTestExecutions[testName])
                {
                    var sampleGroupResult = new SampleGroupResult
                    {
                        SampleGroupName = sampleGroup.Definition.Name,
                        SampleUnit = sampleGroup.Definition.SampleUnit.ToString(),
                        IncreaseIsBetter = sampleGroup.Definition.IncreaseIsBetter,
                        Threshold = sampleGroup.Definition.Threshold,
                        AggregationType = sampleGroup.Definition.AggregationType.ToString(),
                        Percentile = sampleGroup.Definition.Percentile,
                        Min = sampleGroup.Min,
                        Max = sampleGroup.Max,
                        Median = sampleGroup.Median,
                        Average = sampleGroup.Average,
                        StandardDeviation = sampleGroup.StandardDeviation,
                        PercentileValue = sampleGroup.PercentileValue,
                        Sum = sampleGroup.Sum,
                        Zeroes = sampleGroup.Zeroes,
                        SampleCount = sampleGroup.SampleCount,
                        BaselineValue = -1,
                        AggregatedValue = GetAggregatedSampleValue(sampleGroup)
                    };

                    performanceTestResult.SampleGroupResults.Add(sampleGroupResult);
                }
                performanceTestResults.Add(performanceTestResult);
            }
            return performanceTestResults;
        }

        public void UpdateTestResultsBasedOnBaselineResults(List<TestResult> baselineTestResults,
            List<TestResult> testResults, uint sigfig)
        {
            foreach (var testResult in testResults)
            {
                // If the baseline results doesn't have a matching TestName for this result, skip it
                if (baselineTestResults.All(r => r.TestName != testResult.TestName)) continue;

                // Get the corresponding baseline testname samplegroupresults for this result's testname
                var baselineSampleGroupResults = baselineTestResults.First(r => r.TestName == testResult.TestName).SampleGroupResults;

                foreach (var sampleGroupResult in testResult.SampleGroupResults)
                {
                    // if we have a corresponding baseline samplegroupname in this sampleGroupResult, compare them
                    if (baselineSampleGroupResults.Any(sg => sg.SampleGroupName == sampleGroupResult.SampleGroupName))
                    {
                        // Get the baselineSampleGroupResult that corresponds to this SampleGroupResults sample group name
                        var baselineSampleGroupResult = baselineSampleGroupResults.First(sg => sg.SampleGroupName == sampleGroupResult.SampleGroupName);

                        // update this samplegroupresults baselinevalue and threshold to be that of the baselinesamplegroup so we can perform an accurate assessement of
                        // whether or not a regression has occurred.
                        sampleGroupResult.BaselineValue = baselineSampleGroupResult.AggregatedValue;
                        sampleGroupResult.Threshold = baselineSampleGroupResult.Threshold;

                        var res = DeterminePerformanceResult(sampleGroupResult, sigfig);

                        if (res == MeasurementResult.Regression)
                        {
                            sampleGroupResult.Regressed = true;
                            sampleGroupResult.Progressed = false;
                            sampleGroupResult.RegressedKnown = false;
                        }
                        else if (res == MeasurementResult.Progression)
                        {
                            sampleGroupResult.Regressed = false;
                            sampleGroupResult.Progressed = true;
                            sampleGroupResult.RegressedKnown = false;
                        }else if(res = MeasurementResult.RegressionKnown){
                            sampleGroupResult.Regressed = true;
                            sampleGroupResult.Progressed = false;
                            sampleGroupResult.RegressedKnown = true;
                        }
                    }
                }

                if (testResult.SampleGroupResults.Any(r => r.Regressed && r.RegressedKnown == false))
                {
                    testResult.State = (int)TestState.Failure;
                }
            }
        }

        private Dictionary<string, List<SampleGroup>> MergeTestExecutions(PerformanceTestRun performanceTestRun)
        {
            var mergedTestExecutions = new Dictionary<string, List<SampleGroup>>();
            var testNames = performanceTestRun.Results.Select(te => te.TestName).Where(t => !String.IsNullOrEmpty(t)).Distinct().ToList();
            foreach (var testName in testNames)
            {
                var executions = performanceTestRun.Results.Where(te => te.TestName == testName);
                var sampleGroups = new List<SampleGroup>();
                foreach (var execution in executions)
                {
                    foreach (var sampleGroup in execution.SampleGroups)
                    {
                        if (sampleGroups.Any(sg => sg.Definition.Name == sampleGroup.Definition.Name))
                        {
                            sampleGroups.First(sg => sg.Definition.Name == sampleGroup.Definition.Name).Samples
                                .AddRange(sampleGroup.Samples);
                        }
                        else
                        {
                            sampleGroups.Add(sampleGroup);
                        }
                    }
                }

                mergedTestExecutions.Add(testName, sampleGroups);
            }
            return mergedTestExecutions;
        }

        private double GetAggregatedSampleValue(SampleGroup sampleGroup)
        {
            double aggregatedSampleValue;
            switch (sampleGroup.Definition.AggregationType)
            {
                case AggregationType.Average:
                    aggregatedSampleValue = sampleGroup.Average;
                    break;
                case AggregationType.Min:
                    aggregatedSampleValue = sampleGroup.Min;
                    break;
                case AggregationType.Max:
                    aggregatedSampleValue = sampleGroup.Max;
                    break;
                case AggregationType.Median:
                    aggregatedSampleValue = sampleGroup.Median;
                    break;
                case AggregationType.Percentile:
                    aggregatedSampleValue = sampleGroup.PercentileValue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(string.Format("Unhandled aggregation type {0}", sampleGroup.Definition.AggregationType));
            }
            return aggregatedSampleValue;
        }

        private MeasurementResult DeterminePerformanceResult(SampleGroupResult sampleGroup, uint sigFig)
        {
            var measurementResult = MeasurementResult.Neutral;
            var positiveThresholdValue = sampleGroup.BaselineValue + (sampleGroup.BaselineValue * sampleGroup.Threshold);
            var negativeThresholdValue = sampleGroup.BaselineValue - (sampleGroup.BaselineValue * sampleGroup.Threshold);
            positiveThresholdValue += sampleGroup.StandardDeviation;
            negativeThresholdValue -= sampleGroup.StandardDeviation;

            if (sampleGroup.IncreaseIsBetter)
            {
                if (sampleGroup.AggregatedValue.TruncToSigFig(sigFig) < negativeThresholdValue.TruncToSigFig(sigFig))
                {
                    measurementResult = MeasurementResult.Regression;
                    
                    if(sampleGroup.ContainsKnownIssue)
                        measurementResult = MeasurementResult.RegressionKnown;
                }
                if (sampleGroup.AggregatedValue.TruncToSigFig(sigFig) > positiveThresholdValue.TruncToSigFig(sigFig))
                {
                    measurementResult = MeasurementResult.Progression;
                }
            }
            else
            {
                if (sampleGroup.AggregatedValue.TruncToSigFig(sigFig) > positiveThresholdValue.TruncToSigFig(sigFig))
                {
                    measurementResult = MeasurementResult.Regression;

                    if(sampleGroup.ContainsKnownIssue)
                        measurementResult = MeasurementResult.RegressionKnown;
                }
                if (sampleGroup.AggregatedValue.TruncToSigFig(sigFig) < negativeThresholdValue.TruncToSigFig(sigFig))
                {
                    measurementResult = MeasurementResult.Progression;
                }
            }
            return measurementResult;
        }

        public PerformanceTestRunResult CreateTestRunResult(PerformanceTestRun runResults,
            List<TestResult> testResults, string resultName, bool isBaseline = false)
        {
            var performanceTestRunResult = new PerformanceTestRunResult
            {
                ResultName = resultName,
                IsBaseline = isBaseline,
                TestSuite = runResults.TestSuite,
                StartTime =
                    new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(
                        runResults.StartTime),
                TestResults = testResults,
                PlayerSystemInfo = runResults.PlayerSystemInfo,
                EditorVersion = runResults.EditorVersion,
                BuildSettings = runResults.BuildSettings,
                ScreenSettings = runResults.ScreenSettings,
                QualitySettings = runResults.QualitySettings,
                PlayerSettings = runResults.PlayerSettings
            };
            return performanceTestRunResult;
        }
    }
}
