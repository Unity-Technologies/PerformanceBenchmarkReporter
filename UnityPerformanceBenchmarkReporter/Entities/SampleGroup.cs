using System;
using System.Collections.Generic;

namespace UnityPerformanceBenchmarkReporter.Entities
{
    [Serializable]
    public class SampleGroup
    {
        public List<double> Samples;
        public double Min;
        public double Max;
        public double Median;
        public double Average;
        public double StandardDeviation;
        public double PercentileValue;
        public double Sum;
        public int Zeroes;
        public int SampleCount;
        public SampleGroupDefinition Definition;
    }

    [Serializable]
    public class SampleGroupDefinition
    {
        public string Name;
        public SampleUnit SampleUnit;
        public AggregationType AggregationType;
        public double Threshold;
        public bool IncreaseIsBetter;
        public double Percentile;
    }

    public enum AggregationType
    {
        Average = 0,
        Min = 1,
        Max = 2,
        Median = 3,
        Percentile = 4
    }

    public enum SampleUnit
    {
        Nanosecond,
        Microsecond,
        Millisecond,
        Second,
        Byte,
        Kilobyte,
        Megabyte,
        Gigabyte,
        None
    }

}