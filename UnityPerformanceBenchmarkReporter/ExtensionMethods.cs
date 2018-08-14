using System;

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
                var s = Convert.ToString(d);
                var parts = s.Split('.');
                if (parts.Length <= 1 || parts[1].Length <= digits)
                {
                    truncated = d;
                }
                else
                {
                    var newSigDigits = parts[1].Substring(0, (int) (digits == 0 ? digits : parts[1].Length - digits));
                    truncated = Convert.ToUInt32(parts[0] + newSigDigits);
                }
            }

            return truncated;
        }
    }
}
