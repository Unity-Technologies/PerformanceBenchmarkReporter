using System;
using System.Globalization;

namespace UnityPerformanceBenchmarkReporter
{
    public static class ExtensionMethods
    {
        public static double TruncToSigFig(this double d, uint digits)
        {
            double truncated;
            
            if(d == 0)
            {
                truncated = 0;
            }
            else
            {
                var s = Convert.ToString(d, CultureInfo.InvariantCulture);
                var parts = s.Split('.');
                if (parts.Length <= 1 || parts[1].Length <= digits)
                {
                    truncated = d;
                }
                else
                {
                    var newSigDigits = parts[1].Substring(0, (int) digits);
                    var truncString = string.Format("{0}.{1}", parts[0], newSigDigits);
                    truncated = Convert.ToDouble(truncString);
                }
            }

            return truncated;
        }
    }
}
