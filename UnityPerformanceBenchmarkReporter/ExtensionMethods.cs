using System;
using System.Globalization;

namespace UnityPerformanceBenchmarkReporter
{
    public static class ExtensionMethods
    {
        public static double TruncToSigFig(this double value, uint digits)
        {
            if (value == 0)
            {
                return 0;
            }

            var valueStr = Convert.ToString(value, CultureInfo.InvariantCulture);
            var parts = valueStr.Split('.');
            if (parts.Length <= 1 || parts[1].Length <= digits)
            {
                return value;
            }

            var truncString = digits == 0
                ? parts[0]
                : $"{parts[0]}.{parts[1][..(int) digits]}";

            return Convert.ToDouble(truncString, CultureInfo.InvariantCulture);
        }
    }
}
