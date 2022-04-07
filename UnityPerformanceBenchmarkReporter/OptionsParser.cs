using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Options;

namespace UnityPerformanceBenchmarkReporter
{
    public class OptionsParser
    {
        private bool help;
        private readonly string about = "The Unity Performance Benchmark Reporter enables the comparison of performance metric baselines and subsequent performance metrics (as generated using the Unity Test Runner with the Unity Performance Testing Extension) in an html report utilizing graphical visualizations.";

        private readonly string learnMore =
            "To learn more about the Unity Performance Benchmark Reporter visit the Unity Performance Benchmark Reporter GitHub wiki at https://github.com/Unity-Technologies/PerformanceBenchmarkReporter/wiki.";

        private readonly string commandLineOptionFormat =
            "// Command line option format\r\n--results=<Path to a test result XML filename OR directory>... [--baseline=\"Path to a baseline XML filename\"] [--reportdirpath=\"Path to where the report will be written\"]";

        private readonly string example1 = "// Run reporter with one performance test result .xml file\r\n--results=\"G:\\My Drive\\XRPerfRuns\\results\\results.xml\"";

        private readonly string example2 = "// Run reporter against a directory containing one or more performance test result .xml files\r\n--results=\"G:\\My Drive\\XRPerfRuns\\results\"  ";

        private readonly string example3 = "// Run reporter against a directory containing one or more performance test result .xml files, and a baseline .xml result file\r\n--results=\"G:\\My Drive\\XRPerfRuns\\results\" --baseline=\"G:\\My Drive\\XRPerfRuns\\baselines\\baseline.xml\" ";

        public enum ResultType
        {
            Test,
            Baseline
        }

        public void ParseOptions(PerformanceBenchmark performanceBenchmark, IEnumerable<string> args)
        {
            var os = GetOptions(performanceBenchmark);

            try
            {
                var remaining = os.Parse(args);

                if (help)
                {
                    ShowHelp(string.Empty, os);
                }

                if (!performanceBenchmark.ResultFilePaths.Any() && !performanceBenchmark.ResultDirectoryPaths.Any())
                {
                    ShowHelp("Missing required option --results=(filePath|directoryPath)", os);
                }

                if (remaining.Any())
                {
                    var errorMessage = string.Format("Unknown option: '{0}.\r\n'", remaining[0]);
                    ShowHelp(errorMessage, os);
                }
            }
            catch (Exception e)
            {
                ShowHelp(string.Format("Error encountered while parsing option: {0}.\r\n", e.Message), os);
            }
        }

        private OptionSet GetOptions(PerformanceBenchmark performanceBenchmark)
        {
            var optionsSet = new OptionSet();
            optionsSet.Add("?|help|h", "Prints out the options.", option => help = option != null);
            optionsSet.Add("dataversion|version=", "Sets Expected Perf Data Version for Results and Baseline Files (1 = V1 2 = V2). Versions of Unity Perf Framework 2.0 or newer will use the V2 data format. If no arg is provided we assume the format is V2", version => performanceBenchmark.SetDataVersion(version));
            optionsSet.Add("fileformat|format=", "Sets Expected File Format for Results and Baseline Files. If no arg is provided we assume the format is XML", filtype => performanceBenchmark.SetFileType(filtype));
            optionsSet.Add(
                    "results|testresultsxmlsource=",
                    "REQUIRED - Path to a test result XML filename OR directory. Directories are searched resursively. You can repeat this option with multiple result file or directory paths.",
                xmlsource => performanceBenchmark.AddSourcePath(xmlsource, "results", ResultType.Test));
            optionsSet.Add(
                    "baseline|baselinexmlsource:", "OPTIONAL - Path to a baseline XML filename.",
                    xmlsource => performanceBenchmark.AddSourcePath(xmlsource, "baseline", ResultType.Baseline));
            optionsSet.Add(
                    "report|reportdirpath:", "OPTIONAL - Path to where the report will be written. Default is current working directory.",
                    performanceBenchmark.AddReportDirPath);
            optionsSet.Add("failonbaseline",
                "Enable return '1' by the reporter if a baseline is passed in and one or more matching configs is out of threshold. Disabled is default. Use option to enable, or use option and append '-' to explicitly disable.",
                option => performanceBenchmark.FailOnBaseline = option != null);
            return optionsSet;
        }

        private void ShowHelp(string message, OptionSet optionSet)
        {
            if (!string.IsNullOrEmpty(message))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(message);
                Console.ResetColor();
            }

            Console.WriteLine(about + "\r\n");
            Console.WriteLine(learnMore + "\r\n");
            Console.WriteLine("Usage is:" + "\r\n");
            Console.WriteLine(commandLineOptionFormat + "\r\n");
            Console.WriteLine(example1 + "\r\n");
            Console.WriteLine(example2 + "\r\n");
            Console.WriteLine(example3 + "\r\n");
            Console.WriteLine("Options: \r\n");
            optionSet.WriteOptionDescriptions(Console.Error);
            Environment.Exit(-1);
        }
    }
}
