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

                if (!performanceBenchmark.ResultXmlFilePaths.Any() && !performanceBenchmark.ResultXmlDirectoryPaths.Any())
                {
                    ShowHelp("Missing required option --testresultsxmlsource=(filePath|directoryPath)", os);
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
            return new OptionSet()
                .Add("?|help|h", "Prints out the options.", option => help = option != null)
                .Add("testresultsxmlsource=", "REQUIRED - Path to a test result XML filename or directory. Directories are searched resursively. You can repeat this option with multiple result file or directory paths.",
                    xmlsource =>
                    {
                        performanceBenchmark.AddXmlSourcePath(xmlsource, "testresultsxmlsource", ResultType.Test);
                    })
                .Add("baselinexmlsource:", "OPTIONAL - Path to a baseline XML filename or directory. Directories are searched resursively. You can repeat this option with multiple baseline file or directory paths.",
                        xmlsource =>
                        {
                            performanceBenchmark.AddXmlSourcePath(xmlsource, "baselinexmlsource", ResultType.Baseline);
                        })
                .Add("reportdirpath:", "OPTIONAL - Path to directory where the UnityPerformanceBenchmark report will be written. Default is current working directory.",
                    performanceBenchmark.AddReportDirPath);
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
            Console.WriteLine("Usage is:");
            optionSet.WriteOptionDescriptions(Console.Error);
            Environment.Exit(-1);
        }
    }
}
