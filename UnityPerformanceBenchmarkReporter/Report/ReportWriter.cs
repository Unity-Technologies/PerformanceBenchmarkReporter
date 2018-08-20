using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityPerformanceBenchmarkReporter.Entities;

namespace UnityPerformanceBenchmarkReporter.Report
{
    public class ReportWriter
    {
        private readonly string unityPerformanceBenchmarkName = "UnityPerformanceBenchmark";
        private readonly List<PerformanceTestRunResult> perfTestRunResults = new List<PerformanceTestRunResult>();
        private readonly HashSet<string> embeddedResourceNames = new HashSet<string>
        {
            "Chart.bundle.js",
            "styles.css",
            "UnityLogo.png"
        };

        private List<string> distinctTestNames;
        private List<string> distinctSampleGroupNames;
        private PerformanceTestRunResult baselineResults;
        private readonly Regex illegalCharacterScrubberRegex = new Regex("[^0-9a-zA-Z]", RegexOptions.Compiled);
        private uint thisSigFig;
        private bool thisHasBenchmarkResults;
        private MetadataValidator metadataValidator;

        public void WriteReport(List<PerformanceTestRunResult> results, MetadataValidator mdValidator, uint sigFig = 2,
            string reportDirectoryPath = null, bool hasBenchmarkResults = false)
        {
            if (results != null && results.Count > 0)
            {
                metadataValidator = mdValidator;
                thisSigFig = sigFig;
                thisHasBenchmarkResults = hasBenchmarkResults;
                EnsureOrderedResults(results);
                SetDistinctTestNames();
                SetDistinctSampleGroupNames();

                var reportDirectory = EnsureBenchmarkDirectory(reportDirectoryPath);
                WriteEmbeddedResourceFiles(reportDirectory);
                var benchmarkReportFile = GetBenchmarkReportFile(reportDirectory);
                using (var rw = new StreamWriter(benchmarkReportFile))
                {
                    WriteHtmlReport(rw);
                }
            }
            else
            {
                throw new ArgumentNullException(nameof(results), "PerformanceTestRun results list is empty. No report will be written.");
            }
        }

        private FileStream GetBenchmarkReportFile(DirectoryInfo benchmarkDirectory)
        {
            var htmlFileName = Path.Combine(benchmarkDirectory.FullName,
                string.Format("{0}_{1:yyyy-MM-dd_hh-mm-ss-fff}.html", unityPerformanceBenchmarkName, DateTime.Now));
            var benchmarkReportFile = TryCreateHtmlFile(htmlFileName);
            return benchmarkReportFile;
        }

        private DirectoryInfo EnsureBenchmarkDirectory(string reportDirectoryPath)
        {
            var reportDirPath = !string.IsNullOrEmpty(reportDirectoryPath)
                ? reportDirectoryPath
                : Directory.GetCurrentDirectory();
            var benchmarkDirectory = Directory.CreateDirectory(Path.Combine(reportDirPath, unityPerformanceBenchmarkName));
            return benchmarkDirectory;
        }

        private static FileStream TryCreateHtmlFile(string htmlFileName)
        {
            try
            {
                return File.Create(htmlFileName);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Exception thrown while trying to create report file at {0}:\r\n{1}", htmlFileName, e.Message);
                throw;
            }
        }

        private void WriteEmbeddedResourceFiles(DirectoryInfo benchmarkDirectory)
        {
            var assemblyNameParts = Assembly.GetExecutingAssembly().Location.Split('\\');
            var assemblyName = assemblyNameParts[assemblyNameParts.Length - 1].Split('.')[0];

            foreach (var embeddedResourceName in embeddedResourceNames)
            {
                var fileName = Path.Combine(benchmarkDirectory.FullName, embeddedResourceName);

                try
                {
                    WriteResourceToFile(
                        string.Format("{0}.Report.{1}", assemblyName, embeddedResourceName),
                        fileName);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine("Exception thrown while trying to create file from embedded resource at {0}:\r\n{1}", fileName, e.Message);
                    throw;
                }
            }
        }

        private void WriteResourceToFile(string resourceName, string fileName)
        {
            using(var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                using(var file = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    resource.CopyTo(file);
                } 
            }
        }

        private string ScrubStringForSafeForVariableUse(string source)
        {
            return illegalCharacterScrubberRegex.Replace(source, "_");
        }

        private void WriteHtmlReport(StreamWriter streamWriter)
        {
            if (streamWriter == null)
            {
                throw new ArgumentNullException(nameof(streamWriter));
            }

            streamWriter.WriteLine("<!doctype html>");
            streamWriter.WriteLine("<html>");
            WriteHeader(streamWriter);
            WriteBody(streamWriter);
            streamWriter.WriteLine("</html>");
        }

        private void WriteBody(StreamWriter streamWriter)
        {
            streamWriter.WriteLine("<body>");
            WriteLogoWithTitle(streamWriter);
            if (TestRunSettingsExist())
            {
                WriteTestConfig(streamWriter);
            }
            WriteStatMethodTable(streamWriter);
            WriteTestTableWithVisualizations(streamWriter);
            streamWriter.WriteLine("</body>");
        }

        private void WriteTestConfig(StreamWriter streamWriter)
        {
            streamWriter.Write("<table class=\"testconfigtable\">");
            streamWriter.WriteLine("<tr><td>");
            WriteShowTestConfigButton(streamWriter);
            streamWriter.WriteLine("</td></tr>");
            streamWriter.WriteLine("<tr><td>");
            WriteTestConfigTable(streamWriter);
            streamWriter.WriteLine("</td></tr>");
            streamWriter.Write("</table>");
        }

        private void WriteStatMethodTable(StreamWriter streamWriter)
        {
            streamWriter.WriteLine("<table class=\"statMethodTable\">");
            WriteShowFailedTestsCheckbox(streamWriter);
            WriteStatMethodButtons(streamWriter);
            streamWriter.WriteLine("</table>");
        }

        private static void WriteStatMethodButtons(StreamWriter streamWriter)
        {
            streamWriter.WriteLine("<tr><td>");
            streamWriter.WriteLine("<div class=\"buttonheader\">Select statistical method</div>");
            streamWriter.WriteLine(
                "<div class=\"buttondiv\"><button id=\"MinButton\" class=\"button\">Min</button></div>&nbsp<div class=\"buttondiv\"><button id=\"MaxButton\" class=\"button\">Max</button></div>&nbsp<div class=\"buttondiv\"><button id=\"MedianButton\" class=\"initialbutton\">Median</button></div>&nbsp<div class=\"buttondiv\"><button id=\"AverageButton\" class=\"button\">Average</button></div>");
            streamWriter.WriteLine("</td></tr>");
        }

        private void WriteShowFailedTestsCheckbox(StreamWriter streamWriter)
        {
            streamWriter.WriteLine("<tr><td>");
            streamWriter.WriteLine("<div class=\"showedfailedtests\">");
            if (thisHasBenchmarkResults)
            {
                streamWriter.WriteLine("<label id=\"hidefailed\" class=\"containerLabel\">Show failed tests only");
                streamWriter.WriteLine("<input type=\"checkbox\" onclick=\"toggleCanvasWithNoFailures()\">");
            }
            else
            {
                streamWriter.WriteLine("<label id=\"hidefailed\" class=\"disabledContainerLabel\">Show failed tests only");
                streamWriter.WriteLine(
                    "<span class=\"tooltiptext\">No failed tests to show because there are no baseline results.</span>");
                streamWriter.WriteLine("<input type=\"checkbox\" disabled>");
            }
            streamWriter.WriteLine("<span class=\"checkmark\"></span>");
            streamWriter.WriteLine("</label>");
            streamWriter.WriteLine("</div");
            streamWriter.WriteLine("</td></tr>");
        }

        private bool TestRunSettingsExist()
        {
            var firstResult = perfTestRunResults.First();
            return firstResult.PlayerSystemInfo != null
                    || firstResult.PlayerSettings != null
                    || firstResult.QualitySettings != null
                    || firstResult.ScreenSettings != null
                    || firstResult.BuildSettings != null
                    || firstResult.EditorVersion != null;
        }

        private void WriteShowTestConfigButton(StreamWriter rw)
        {
            rw.WriteLine("<button id=\"toggleconfig\" class=\"button\" onclick=\"showTestConfiguration()\">Show Test Configuration</button>");
        }

        private void WriteJavaScript(StreamWriter rw)
        {
            rw.WriteLine("<script>");
            rw.WriteLine("var failColor = \"rgba(255, 99, 132,0.5)\";");
            rw.WriteLine("var passColor = \"rgba(54, 162, 235,0.5)\";");
            rw.WriteLine("var baselineColor = \"rgb(255, 159, 64)\";");

            WriteShowTestConfigurationButton(rw);
            WriteToggleCanvasWithNoFailures(rw);

            WriteTestRunArray(rw);
            WriteValueArrays(rw);

            foreach (var distinctTestName in distinctTestNames)
            {
                var resultsForThisTest = GetResultsForThisTest(distinctTestName);
                foreach (var distinctSampleGroupName in distinctSampleGroupNames)
                {
                    if (!SampleGroupHasSamples(resultsForThisTest, distinctSampleGroupName)) continue;

                    var canvasId = GetCanvasId(distinctTestName, distinctSampleGroupName);
                    var format = string.Format("var {0}_data = {{", canvasId);
                    rw.WriteLine(format);
                    rw.WriteLine("	labels: testRuns,");
                    rw.WriteLine("	datasets: [{");
                    var resultColors = new StringBuilder();
                    resultColors.Append("		backgroundColor: [");

                    foreach (var testResult in resultsForThisTest)
                    {
                        if (testResult.SampleGroupResults.Any(r =>
                            ScrubStringForSafeForVariableUse(r.SampleGroupName).Equals(distinctSampleGroupName)))
                        {
                            var sampleGroupResult = testResult.SampleGroupResults.First(r =>
                                ScrubStringForSafeForVariableUse(r.SampleGroupName).Equals(distinctSampleGroupName));
                            resultColors.Append(sampleGroupResult.Regressed ? "failColor, " : "passColor, ");
                        }
                        else
                        {
                            resultColors.Append("passColor, ");
                        }
                    }

                    var sampleUnit = GetSampleUnit(resultsForThisTest, distinctSampleGroupName);

                    // remove trailing comma
                    resultColors.Length = resultColors.Length - 2;
                    resultColors.Append("],");
                    rw.WriteLine(resultColors.ToString());
                    rw.WriteLine("		borderWidth: 1,");
                    rw.WriteLine("		label: \"" + (sampleUnit.Equals("None") ? distinctSampleGroupName : sampleUnit) + "\",");
                    rw.WriteLine("		legend: {");
                    rw.WriteLine("display: false,");
                    rw.WriteLine("		},");
                    rw.WriteLine("		data: {0}", string.Format("{0}_Aggregated_Values", canvasId));
                    rw.WriteLine("	}");
                    if (baselineResults != null)
                    {
                        rw.WriteLine("	,{");
                        rw.WriteLine("		borderColor: baselineColor,");
                        rw.WriteLine("		borderWidth: 2,");
                        rw.WriteLine("		fill: false,");
                        rw.WriteLine("		pointStyle: 'line',");
                        rw.WriteLine("		label: \"" + (sampleUnit.Equals("None") ? "Baseline " + distinctSampleGroupName : "Baseline " + sampleUnit) + "\",");
                        rw.WriteLine("		legend: {");
                        rw.WriteLine("display: false,");
                        rw.WriteLine("		},");
                        rw.WriteLine("		data: {0}", string.Format("{0}_Baseline_Values,", canvasId));
                        rw.WriteLine("		type: 'line'}");
                    }

                    rw.WriteLine("	]");
                    rw.WriteLine("};");
                }
            }

            rw.WriteLine("window.onload = function() {");
            foreach (var distinctTestName in distinctTestNames)
            {
                var resultsForThisTest = GetResultsForThisTest(distinctTestName);
                foreach (var distinctSampleGroupName in distinctSampleGroupNames)
                {
                    if (!SampleGroupHasSamples(resultsForThisTest, distinctSampleGroupName))
                    {
                        continue;
                    }
                    WriteChartConfigForSampleGroup(rw, resultsForThisTest, distinctSampleGroupName, distinctTestName);
                }
            }
            WriteStatMethodButtonEventListeners(rw);
            rw.WriteLine("};");
            rw.WriteLine("</script>");
        }

        private void WriteStatMethodButtonEventListeners(StreamWriter rw)
        {
            var statisticalMethods = new List<string> {"Min", "Max", "Median", "Average"};
            foreach (var thisStatMethod in statisticalMethods)
            {
                rw.WriteLine("	document.getElementById('{0}Button').addEventListener('click', function()", thisStatMethod);
                rw.WriteLine("	{");
                foreach (var distinctTestName in distinctTestNames)
                {
                    var resultsForThisTest = GetResultsForThisTest(distinctTestName);
                    foreach (var distinctSampleGroupName in distinctSampleGroupNames)
                    {
                        if (!SampleGroupHasSamples(resultsForThisTest, distinctSampleGroupName)) continue;

                        var canvasId = GetCanvasId(distinctTestName, distinctSampleGroupName);
                        var sampleUnit = GetSampleUnit(resultsForThisTest, distinctSampleGroupName);

                        rw.WriteLine("window.{0}.options.scales.yAxes[0].scaleLabel.labelString = \"{1} {2}\";", canvasId,
                            thisStatMethod, !sampleUnit.Equals("None") ? sampleUnit : distinctSampleGroupName);
                        rw.WriteLine("{0}_data.datasets[0].data = {0}_{1}_Values;", canvasId, thisStatMethod);
                        rw.WriteLine("window.{0}.update();", canvasId);
                    }

                    rw.WriteLine("var a = document.getElementById('{0}Button');", thisStatMethod);
                    rw.WriteLine("a.style.backgroundColor = \"#2196F3\";");
                    var count = 98;
                    foreach (var statMethod in statisticalMethods.Where(m=>!m.Equals(thisStatMethod)))
                    {
                        var varName = Convert.ToChar(count);
                        rw.WriteLine("var {0} = document.getElementById('{1}Button');", varName, statMethod);
                        rw.WriteLine("{0}.style.backgroundColor = \"#3e6892\";", varName);
                        count++;
                    }
                }

                rw.WriteLine("	 });");
            }
        }

        private void WriteChartConfigForSampleGroup(StreamWriter rw, List<TestResult> resultsForThisTest, string distinctSampleGroupName,
            string distinctTestName)
        {
            var aggregationType = GetAggregationType(resultsForThisTest, distinctSampleGroupName);
            var sampleUnit = GetSampleUnit(resultsForThisTest, distinctSampleGroupName);
            var threshold = GetThreshold(resultsForThisTest, distinctSampleGroupName);
            var canvasId = GetCanvasId(distinctTestName, distinctSampleGroupName);

            rw.WriteLine("	var ctx{0} = document.getElementById('{0}').getContext('2d');", canvasId);
            rw.WriteLine("	window.{0} = new Chart(ctx{0}, {{", canvasId);
            rw.WriteLine("		type: 'bar',");
            rw.WriteLine("		data: {0}_data,", canvasId);
            rw.WriteLine("		options: {");
            rw.WriteLine("tooltips:");
            rw.WriteLine("{");
            rw.WriteLine("    mode: 'index',");
            rw.WriteLine("    callbacks:");
            rw.WriteLine("    {");
            rw.WriteLine("        footer: function(tooltipItems, data) {");
            rw.WriteLine("		var std = {0}_Stdev_Values[tooltipItems[0].index];", canvasId);
            rw.WriteLine("            return 'Threshold: " + threshold + " Standard deviation: ' + std;");
            rw.WriteLine("        },");
            rw.WriteLine("   },");
            rw.WriteLine("    footerFontStyle: 'normal'");
            rw.WriteLine("},");
            rw.WriteLine("legend: { display: false},");
            rw.WriteLine("maintainAspectRatio: false,");
            rw.WriteLine("scales: {");
            rw.WriteLine("	yAxes: [{");
            rw.WriteLine("		display: true,");
            rw.WriteLine("		scaleLabel:");
            rw.WriteLine("		{");
            rw.WriteLine("		    display: true,");
            rw.WriteLine("		    labelString: \"{0} {1}\"", aggregationType, !sampleUnit.Equals("None") ? sampleUnit : distinctSampleGroupName);
            rw.WriteLine("		},");
            rw.WriteLine("		ticks: {");
            rw.WriteLine("suggestedMax: .001,");
            rw.WriteLine("suggestedMin: .0");
            rw.WriteLine("		}");
            rw.WriteLine("	}]");
            rw.WriteLine("},");
            rw.WriteLine("responsive: true,");
            rw.WriteLine("responsiveAnimationDuration: 0,");
            rw.WriteLine("title: {");
            rw.WriteLine("	display: true,");
            rw.WriteLine("	text: \"{0}\"", distinctSampleGroupName);
            rw.WriteLine("}");
            rw.WriteLine("		}");
            rw.WriteLine("	});");
            rw.WriteLine("	");
        }

        private void WriteShowTestConfigurationButton(StreamWriter rw)
        {
            rw.WriteLine("function showTestConfiguration() {");
            rw.WriteLine("	var x = document.getElementById(\"testconfig\");");
            rw.WriteLine("	if (x.style.display === \"\" || x.style.display === \"none\") {");
            rw.WriteLine("		x.style.display = \"block\";");
rw.WriteLine("	document.getElementById(\"toggleconfig\").innerHTML=\"Hide Test Configuration\";");
            rw.WriteLine("	} else {");
            rw.WriteLine("		x.style.display = \"none\";");
rw.WriteLine("	document.getElementById(\"toggleconfig\").innerHTML=\"Show Test Configuration\";");
            rw.WriteLine("	}");
            rw.WriteLine("}");
        }

        private void WriteToggleCanvasWithNoFailures(StreamWriter rw)
        {
            rw.WriteLine("function toggleCanvasWithNoFailures() {");
            rw.WriteLine("	var x = document.getElementsByClassName(\"nofailures\");");
            rw.WriteLine("              for(var i = 0; i < x.length; i++)");
            rw.WriteLine("              {"); 
            rw.WriteLine("	    if (x[i].style.display === \"none\") {");
            rw.WriteLine("		    x[i].getAttribute('style');");
            rw.WriteLine("		    x[i].removeAttribute('style');");
            rw.WriteLine("	    } else {");
            rw.WriteLine("		    x[i].style.display = \"none\";");
            rw.WriteLine("	    }");
            rw.WriteLine("	}");
            rw.WriteLine("}");
        }

        private void WriteHeader(StreamWriter rw)
        {
            rw.WriteLine("<head>");
            rw.WriteLine("<title>Unity Performance Benchmark Report</title>");
            rw.WriteLine("<script src=\"Chart.bundle.js\"></script>");
            rw.WriteLine("<link rel=\"stylesheet\" href=\"styles.css\">");
            rw.WriteLine("<style>");
            rw.WriteLine("		canvas {");
            rw.WriteLine("-moz-user-select: none;");
            rw.WriteLine("-webkit-user-select: none;");
            rw.WriteLine("-ms-user-select: none;");
            rw.WriteLine("		}");
            rw.WriteLine("</style>");
            WriteJavaScript(rw);
            rw.WriteLine("</head>");
        }

        private void WriteLogoWithTitle(StreamWriter rw)
        {
            rw.WriteLine("<table class=\"titletable\">");
            rw.WriteLine("<tr><td class=\"logocell\"><img src=\"UnityLogo.png\" alt=\"Unity\" class=\"logo\"></td></tr>");
            rw.WriteLine("<tr><td class=\"titlecell\"><div class=\"title\"><h1>Performance Benchmark Report</h1></div></td></tr>");
            rw.WriteLine("</table>");
        }

        private void WriteTestTableWithVisualizations(StreamWriter rw)
        {
            rw.WriteLine("<table class=\"visualizationTable\">");
            foreach (var distinctTestName in distinctTestNames)
            {
                WriteResultForThisTest(rw, distinctTestName);
            }
            rw.WriteLine("</table>");
        }

        private void WriteResultForThisTest(StreamWriter rw, string distinctTestName)
        {
            var resultsForThisTest = GetResultsForThisTest(distinctTestName);
            var noTestRegressions = IsNoTestFailures(resultsForThisTest);
            rw.WriteLine(noTestRegressions ? "<tr class=\"nofailures\"><td><hr></td></tr>" : "<tr><td><hr></td></tr>");
            rw.WriteLine(noTestRegressions ? "<tr class=\"nofailures\">" : "<tr>");
            rw.WriteLine("<td class=\"testnamecell chartcell\">");
            rw.WriteLine(
                noTestRegressions
                    ? "<div class=\"testname nofailures\"><h3>{0}</h3> </div>"
                    : "<div class=\"testname\"><h3>{0}</h3> </div>", distinctTestName);
            rw.WriteLine("</td></tr>");
            rw.WriteLine(noTestRegressions ? "<tr class=\"nofailures\"><td><hr></td></tr>" : "<tr><td><hr></td></tr>");

            foreach (var distinctSampleGroupName in distinctSampleGroupNames)
            {
                WriteResultForThisSampleGroup(rw, distinctTestName, resultsForThisTest, distinctSampleGroupName, noTestRegressions);
            }
        }

        private void WriteResultForThisSampleGroup(StreamWriter rw, string distinctTestName,
            List<TestResult> resultsForThisTest,
            string distinctSampleGroupName, bool noTestRegressions)
        {
            if (!SampleGroupHasSamples(resultsForThisTest, distinctSampleGroupName))
            {
                return;
            }

            var canvasId = GetCanvasId(distinctTestName, distinctSampleGroupName);
            var noSampleGroupRegressions = !SampleGroupHasRegressions(resultsForThisTest, distinctSampleGroupName);
            rw.WriteLine(
                    noSampleGroupRegressions
                        ? "<tr class=\"nofailures\"><td class=\"chartcell nofailures\"><div id=\"container\" class=\"container nofailures\">"
                        : "<tr><td class=\"chartcell\"><div id=\"container\" class=\"container\">");
            rw.WriteLine(
                noSampleGroupRegressions
                    ? "<canvas class=\"nofailures canvas\" id=\"{0}\"></canvas>"
                    : "<canvas class=\"canvas\" id=\"{0}\"></canvas>", canvasId);

            rw.WriteLine("</div></td></tr>");
        }

        private bool IsNoTestFailures(List<TestResult> resultsForThisTest)
        {
            bool noTestFailures = true;
            foreach (var distinctSampleGroupName2 in distinctSampleGroupNames)
            {
                if (!SampleGroupHasSamples(resultsForThisTest, distinctSampleGroupName2)) continue;
                noTestFailures = noTestFailures && !SampleGroupHasRegressions(resultsForThisTest, distinctSampleGroupName2);
            }

            return noTestFailures;
        }

        private void WriteTestRunArray(StreamWriter rw)
        {
            var runsString = new StringBuilder();
            runsString.Append("var testRuns = [");

            // Write remaining values
            foreach (var performanceTestRunResult in perfTestRunResults)
            {
                runsString.Append(string.Format("'{0}', ", performanceTestRunResult.ResultName));
            }

            // Remove trailing comma and space
            runsString.Length = runsString.Length - 2;
            runsString.Append("];");
            rw.WriteLine(runsString.ToString());
        }

        private void WriteValueArrays(StreamWriter rw)
        {
            foreach (var distinctTestName in distinctTestNames)
            {
                var resultsForThisTest = GetResultsForThisTest(distinctTestName);
                foreach (var distinctSampleGroupName in distinctSampleGroupNames)
                {
                    if (!SampleGroupHasSamples(resultsForThisTest, distinctSampleGroupName)) continue;

                    var aggregatedValuesArrayName =
                        string.Format("{0}_{1}_Aggregated_Values", distinctTestName, distinctSampleGroupName);
                    var medianValuesArrayName =
                        string.Format("{0}_{1}_Median_Values", distinctTestName, distinctSampleGroupName);
                    var minValuesArrayName = string.Format("{0}_{1}_Min_Values", distinctTestName, distinctSampleGroupName);
                    var maxValuesArrayName = string.Format("{0}_{1}_Max_Values", distinctTestName, distinctSampleGroupName);
                    var avgValuesArrayName = string.Format("{0}_{1}_Average_Values", distinctTestName, distinctSampleGroupName);
                    var stdevValuesArrayName = string.Format("{0}_{1}_Stdev_Values", distinctTestName, distinctSampleGroupName);

                    var baselineValuesArrayName =
                        string.Format("{0}_{1}_Baseline_Values", distinctTestName, distinctSampleGroupName);

                    var aggregatedValuesArrayString = new StringBuilder();
                    aggregatedValuesArrayString.Append(string.Format("var {0} = [", aggregatedValuesArrayName));

                    var medianValuesArrayString = new StringBuilder();
                    medianValuesArrayString.Append(string.Format("var {0} = [", medianValuesArrayName));

                    var minValuesArrayString = new StringBuilder();
                    minValuesArrayString.Append(string.Format("var {0} = [", minValuesArrayName));

                    var maxValuesArrayString = new StringBuilder();
                    maxValuesArrayString.Append(string.Format("var {0} = [", maxValuesArrayName));

                    var avgValuesArrayString = new StringBuilder();
                    avgValuesArrayString.Append(string.Format("var {0} = [", avgValuesArrayName));

                    var stdevValuesArrayString = new StringBuilder();
                    stdevValuesArrayString.Append(string.Format("var {0} = [", stdevValuesArrayName));

                    var baselineValuesArrayString = new StringBuilder();
                    baselineValuesArrayString.Append(string.Format("var {0} = [", baselineValuesArrayName));

                    foreach (var performanceTestRunResult in perfTestRunResults)
                    {
                        if (performanceTestRunResult.TestResults.Any(r => ScrubStringForSafeForVariableUse(r.TestName).Equals(distinctTestName)))
                        {
                            var testResult =
                                performanceTestRunResult.TestResults.First(r =>
                                    ScrubStringForSafeForVariableUse(r.TestName).Equals(distinctTestName));
                            var sgResult =
                                testResult.SampleGroupResults.FirstOrDefault(r =>
                                    ScrubStringForSafeForVariableUse(r.SampleGroupName).Equals(distinctSampleGroupName));
                            aggregatedValuesArrayString.Append(string.Format("'{0}', ",
                                sgResult != null ? sgResult.AggregatedValue.ToString("F" + thisSigFig) : ""));
                            medianValuesArrayString.Append(string.Format("'{0}', ",
                                sgResult != null ? sgResult.Median.ToString("F" + thisSigFig) : ""));
                            minValuesArrayString.Append(string.Format("'{0}', ",
                                sgResult != null ? sgResult.Min.ToString("F" + thisSigFig) : ""));
                            maxValuesArrayString.Append(string.Format("'{0}' ,",
                                sgResult != null ? sgResult.Max.ToString("F" + thisSigFig) : ""));
                            avgValuesArrayString.Append(string.Format("'{0}' ,",
                                sgResult != null ? sgResult.Average.ToString("F" + thisSigFig) : ""));
                            stdevValuesArrayString.Append(string.Format("'{0}' ,",
                                sgResult != null ? sgResult.StandardDeviation.ToString("F" + thisSigFig) : ""));
                            baselineValuesArrayString.Append(string.Format("'{0}' ,",
                                sgResult != null ? sgResult.BaselineValue.ToString("F" + thisSigFig) : ""));
                        }
                    }

                    // Remove trailing commas from string builder
                    aggregatedValuesArrayString.Length = aggregatedValuesArrayString.Length - 2;
                    medianValuesArrayString.Length = medianValuesArrayString.Length - 2;
                    minValuesArrayString.Length = minValuesArrayString.Length - 2;
                    maxValuesArrayString.Length = maxValuesArrayString.Length - 2;
                    avgValuesArrayString.Length = avgValuesArrayString.Length - 2;
                    stdevValuesArrayString.Length = stdevValuesArrayString.Length - 2;
                    baselineValuesArrayString.Length = baselineValuesArrayString.Length - 2;

                    aggregatedValuesArrayString.Append("];");
                    medianValuesArrayString.Append("];");
                    minValuesArrayString.Append("];");
                    maxValuesArrayString.Append("];");
                    avgValuesArrayString.Append("];");
                    stdevValuesArrayString.Append("];");
                    baselineValuesArrayString.Append("];");

                    rw.WriteLine(aggregatedValuesArrayString.ToString());
                    rw.WriteLine(medianValuesArrayString.ToString());
                    rw.WriteLine(minValuesArrayString.ToString());
                    rw.WriteLine(maxValuesArrayString.ToString());
                    rw.WriteLine(avgValuesArrayString.ToString());
                    rw.WriteLine(stdevValuesArrayString.ToString());
                    rw.WriteLine(baselineValuesArrayString.ToString());
                }
            }
        }

        private void WriteTestConfigTable(StreamWriter rw)
        {
            var firstResult = perfTestRunResults.First();

            rw.WriteLine("<div ng-hide=\"isCollapsed\" select-on-click>");
            rw.WriteLine("<div id=\"testconfig\" class=\"testconfig\"> ");

            if (firstResult.PlayerSystemInfo != null)
            {
                WriteClassNameWithFields<PlayerSystemInfo>(rw, firstResult.PlayerSystemInfo, metadataValidator.PlayerSystemInfoResultFiles, metadataValidator.MismatchedPlayerSystemInfoValues);
            }

            if (firstResult.PlayerSettings != null)
            {
                WriteClassNameWithFields<PlayerSettings>(rw, firstResult.PlayerSettings, metadataValidator.PlayerSettingsResultFiles, metadataValidator.MismatchedPlayerSettingsValues);
            }

            if (firstResult.QualitySettings != null)
            {
                WriteClassNameWithFields<QualitySettings>(rw, firstResult.QualitySettings, metadataValidator.QualitySettingsResultFiles, metadataValidator.MismatchedQualitySettingsValues);
            }

            if (firstResult.ScreenSettings != null)
            {
                WriteClassNameWithFields<ScreenSettings>(rw, firstResult.ScreenSettings, metadataValidator.ScreenSettingsResultFiles, metadataValidator.MismatchedScreenSettingsValues);
            }

            if (firstResult.BuildSettings != null)
            {
                WriteClassNameWithFields<BuildSettings>(rw, firstResult.BuildSettings, metadataValidator.BuildSettingsResultFiles, metadataValidator.MismatchedBuildSettingsValues);
            }

            if (firstResult.EditorVersion != null)
            {
                WriteClassNameWithFields<EditorVersion>(rw, firstResult.EditorVersion, metadataValidator.EditorVersionResultFiles, metadataValidator.MismatchedEditorVersionValues, new[] { "DateSeconds", "RevisionValue" }, true);
            }
            
            rw.WriteLine("</div>");
            rw.WriteLine("</div>");
        }

        private void WriteClassNameWithFields<T>(StreamWriter rw, object instance, string[] resultFiles,
            Dictionary<string, Dictionary<string, string>> mismatchedValues, string[] excludedFields = null, bool wideLayout = false)
        {
            var thisObject = (T)instance;
            rw.WriteLine("<div><hr></div><div class=\"typename\">{0}</div><div><hr></div>", thisObject.GetType().Name);
            rw.WriteLine(wideLayout ? "<div class=\"systeminfowide\"><pre>" : "<div class=\"systeminfo\"><pre>");

            var sb = new StringBuilder();
            foreach (var field in thisObject.GetType().GetFields())
            {
                
                if (excludedFields != null && excludedFields.Contains(field.Name))
                {
                    continue;
                }
                sb.Append("<div class=\"fieldgroup\">");

                // if field is an IEnumberable, enumerate and append each value to the sb
                if (typeof(IEnumerable).IsAssignableFrom(field.FieldType) && field.FieldType != typeof(string))
                {
                    sb.Append(string.Format("<div><div class=\"fieldname\"><pre>{0}</pre></div><div class=\"fieldvalue\"><pre>", field.Name));

                    //if (mismatchedValues.Count > 0 && mismatchedValues.ContainsKey(field.Name))
                    //{
                        foreach (var enumerable in (IEnumerable)field.GetValue(thisObject))
                        {
                            sb.Append(enumerable + ",");
                        }

                        if (sb.ToString().EndsWith(','))
                        {
                            // trim trailing comma
                            sb.Length--;
                        }
                    //}
                    
                    sb.Append("</pre></div></div>");
                }
                else
                {
                    sb.Append(string.Format("<div class=\"fieldname\">{0}</div>", field.Name));
                    if (mismatchedValues.Count > 0 && mismatchedValues.ContainsKey(field.Name))
                    {
                        var mismatchedValue = mismatchedValues[field.Name];

                        sb.Append("<div class=\"fieldvaluewarning\">");
                        sb.Append("<table class=\"warningtable\">");
                        sb.Append("<tr><th>Value</th><th>Result File</th><th>Path</th></tr>");
                        
                        for (int i = 0; i < resultFiles.Length; i++)
                        {
                            
                            var resultFile = resultFiles[i];
                            var value = mismatchedValue.Any(kv => kv.Key.Equals(resultFile)) ? 
                                mismatchedValue.First(kv => kv.Key.Equals(resultFile)).Value : 
                                mismatchedValue.First(kv => kv.Key.Equals(resultFiles[0])).Value;

                            var pathParts = resultFile.Split('\\');
                            var path = string.Join('\\', pathParts.Take(pathParts.Length - 1));

                            sb.Append(string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>", value, pathParts[pathParts.Length - 1], path));
                        }
                        
                        sb.Append("</table></div>");
                    }
                    else
                    {
                        sb.Append(string.Format("<div class=\"fieldvalue\">{0}</div>", field.GetValue(thisObject)));
                    }

                    
                }
                sb.Append("</div>");
            }
            rw.WriteLine(sb.ToString());
            rw.WriteLine("</pre></div>");
        }

        private List<TestResult> GetResultsForThisTest(string distinctTestName)
        {
            var resultsForThisTest = new List<TestResult>();
            foreach (var performanceTestRunResult in perfTestRunResults)
            {
                if (performanceTestRunResult.TestResults.Any(r => ScrubStringForSafeForVariableUse(r.TestName).Equals(distinctTestName)))
                {
                    resultsForThisTest.Add(
                        performanceTestRunResult.TestResults.First(r => ScrubStringForSafeForVariableUse(r.TestName).Equals(distinctTestName)));
                }
            }

            return resultsForThisTest;
        }

        private string GetSampleUnit(List<TestResult> resultsForThisTest, string sampleGroupName)
        {
            var sampleUnit = "";
            if (resultsForThisTest.First().SampleGroupResults
                .Any(sg => ScrubStringForSafeForVariableUse(sg.SampleGroupName) == sampleGroupName))
            {
                sampleUnit = resultsForThisTest.First().SampleGroupResults
                    .First(sg => ScrubStringForSafeForVariableUse(sg.SampleGroupName) == sampleGroupName).SampleUnit;
            }
            return sampleUnit;
        }

        private double GetThreshold(List<TestResult> resultsForThisTest, string sampleGroupName)
        {
            var threshold = 0.0;
            if (resultsForThisTest.First().SampleGroupResults
                .Any(sg => ScrubStringForSafeForVariableUse(sg.SampleGroupName) == sampleGroupName))
            {
                threshold = resultsForThisTest.First().SampleGroupResults
                    .First(sg => ScrubStringForSafeForVariableUse(sg.SampleGroupName) == sampleGroupName).Threshold;
            }
            return threshold;
        }

        private string GetAggregationType(List<TestResult> resultsForThisTest, string sampleGroupName)
        {
            var aggregationType = "";
            if (resultsForThisTest.First().SampleGroupResults
                .Any(sg => ScrubStringForSafeForVariableUse(sg.SampleGroupName) == sampleGroupName))
            {
                aggregationType = resultsForThisTest.First().SampleGroupResults
                    .First(sg => ScrubStringForSafeForVariableUse(sg.SampleGroupName) == sampleGroupName).AggregationType;
            }
            return aggregationType;
        }

        private void EnsureOrderedResults(List<PerformanceTestRunResult> results)
        {
            baselineResults = results.FirstOrDefault(r => r.IsBaseline);
            if (baselineResults != null)
            {
                perfTestRunResults.Add(baselineResults);
            }

            var tempResults = results.Where(r => !r.IsBaseline).ToList();
            tempResults.Sort((x, y) => DateTime.Compare(x.StartTime, y.StartTime));
            perfTestRunResults.AddRange(tempResults);
        }

        private bool SampleGroupHasSamples(IEnumerable<TestResult> resultsForThisTest, string distinctSampleGroupName)
        {
            return resultsForThisTest.First().SampleGroupResults.Any(sg => ScrubStringForSafeForVariableUse(sg.SampleGroupName) == distinctSampleGroupName);
        }

        private bool SampleGroupHasRegressions(IEnumerable<TestResult> resultsForThisTest, string distinctSampleGroupName)
        {
            var failureInSampleGroup = resultsForThisTest.SelectMany(r => r.SampleGroupResults).Where(sg => ScrubStringForSafeForVariableUse(sg.SampleGroupName) == distinctSampleGroupName ).Any(r => r.Regressed);

            return failureInSampleGroup;
        }

        private string GetCanvasId(string distinctTestName, string distinctSgName)
        {
            return string.Format("{0}_{1}", distinctTestName, distinctSgName);
        }

        private void SetDistinctSampleGroupNames()
        {
            var sgNames = new List<string>();

            foreach (var performanceTestRunResult in perfTestRunResults)
            {
                foreach (var testResult in performanceTestRunResult.TestResults)
                {
                    sgNames.AddRange(testResult.SampleGroupResults.Select(r => r.SampleGroupName));
                }
            }

            var tempDistinctSgNames = sgNames.Distinct().ToArray();
            for (var i = 0; i < tempDistinctSgNames.Length; i++)
            {
                tempDistinctSgNames[i] = ScrubStringForSafeForVariableUse(tempDistinctSgNames[i]);
            }
            distinctSampleGroupNames = tempDistinctSgNames.ToList();
            distinctSampleGroupNames.Sort();
        }

        private void SetDistinctTestNames()
        {
            var testNames = new List<string>();

            foreach (var performanceTestRunResult in perfTestRunResults)
            {
                testNames.AddRange(performanceTestRunResult.TestResults.Select(tr => tr.TestName));
            }

            var tempDistinctTestNames = testNames.Distinct().ToArray();
            for (var i = 0; i < tempDistinctTestNames.Length; i++)
            {
                tempDistinctTestNames[i] = ScrubStringForSafeForVariableUse(tempDistinctTestNames[i]);
            }

            distinctTestNames = tempDistinctTestNames.ToList();
            distinctTestNames.Sort();
        }
    }
}
