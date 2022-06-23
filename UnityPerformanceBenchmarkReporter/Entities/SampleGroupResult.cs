
namespace UnityPerformanceBenchmarkReporter.Entities
{
    public class SampleGroupResult
    {
        public string SampleGroupName;
        public string SampleUnit;
        public double AggregatedValue;
        public double BaselineValue;
        public double Threshold;
        public bool IncreaseIsBetter;
        public string AggregationType;
        public double Percentile;
        public bool Regressed;
        public bool Progressed;
        public bool RegressedKnown;
        public double Min;
        public double Max;
        public double Median;
        public double Average;
        public double StandardDeviation;
        public double PercentileValue;
        public double Sum;
        public int Zeroes;
        public int SampleCount;
    }
}
