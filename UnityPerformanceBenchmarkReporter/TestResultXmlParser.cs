using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json;
using UnityPerformanceBenchmarkReporter.Entities;

namespace UnityPerformanceBenchmarkReporter
{
    public class TestResultXmlParser
    {
        public PerformanceTestRun GetPerformanceTestRunFromXml(string resultXmlFileName)
        {
            ValidateInput(resultXmlFileName);
            var xmlDocument = TryLoadResultXmlFile(resultXmlFileName);
            var performanceTestRun = TryParseXmlToPerformanceTestRun(xmlDocument);
            return performanceTestRun;
        }

        public void ValidateInput(string resultXmlFileName)
        {
            if (string.IsNullOrEmpty(resultXmlFileName))
            {
                throw new ArgumentNullException(resultXmlFileName, nameof(resultXmlFileName));
            }

            if (!File.Exists(resultXmlFileName))
            {
                throw new FileNotFoundException("Result file not found; {0}", resultXmlFileName);
            }
        }

        private XDocument TryLoadResultXmlFile(string resultXmlFileName)
        {
            try
            {
                return XDocument.Load(resultXmlFileName);
            }
            catch (Exception e)
            {
                var errMsg = string.Format("Failed to load xml result file: {0}", resultXmlFileName);
                WriteExceptionConsoleErrorMessage(errMsg, e);
                throw;
            }
        }

        public PerformanceTestRun TryParseXmlToPerformanceTestRun(XDocument xmlDocument)
        {
            var output = xmlDocument.Descendants("output").ToArray();
            if (output == null || !output.Any())
            {
                throw new Exception("The xmlDocument passed to the TryParseXmlToPerformanceTestRun method does not have any \'ouput\' xml tags needed for correct parsing.");
            }

            var run = new PerformanceTestRun();
            DeserializeTestResults(output, run);
            DeserializeMetadata(output, run);
            
            return run;
        }

        private void DeserializeTestResults(IEnumerable<XElement> output, PerformanceTestRun run)
        {
            foreach (var element in output)
            {
                foreach (var line in element.Value.Split('\n'))
                {
                    var json = GetJsonFromHashtag("performancetestresult", line);
                    if (json == null)
                    {
                        continue;
                    }

                    var result = TryDeserializePerformanceTestResultJsonObject(json);
                    if (result != null)
                    {
                        run.Results.Add(result);
                    }                    
                }
            }
        }

        private void DeserializeMetadata(IEnumerable<XElement> output, PerformanceTestRun run)
        {
            foreach (var element in output)
            {
                var elements = element.Value.Split('\n');
                if(elements.Where(e => e.Length > 0 && e.Substring(0, 2).Equals("##")).Any())
                {
                    var line = elements.Where(e => e.Length > 0 && e.Substring(0, 2).Equals("##")).First();

                    var json = GetJsonFromHashtag("performancetestruninfo", line);

                    // This is the happy case where we have a performancetestruninfo json object
                    if (json != null)
                    {
                        var result = TryDeserializePerformanceTestRunJsonObject(json);
                        if (result != null)
                        {
                            run.TestSuite = result.TestSuite;
                            run.EditorVersion = result.EditorVersion;
                            run.QualitySettings = result.QualitySettings;
                            run.ScreenSettings = result.ScreenSettings;
                            run.BuildSettings = result.BuildSettings;
                            run.PlayerSettings = result.PlayerSettings;
                            run.PlayerSystemInfo = result.PlayerSystemInfo;
                            run.StartTime = result.StartTime;
                            // @TODO fix end time, does it matter for now?
                            run.EndTime = run.StartTime;
                        }
                    }
                    // Unhappy case where we couldn't find a performancetestruninfo object
                    // This could be because we have missing metadata for the test run
                    // In this case, we try to look for a performancetestresult json object
                    // We should have at least startime metadata  that we can use to correctly
                    // display the test results on the x-axis of the chart
                    else
                    {
                        json = GetJsonFromHashtag("performancetestresult", line);
                        if (json != null)
                        {
                            var result = TryDeserializePerformanceTestRunJsonObject(json);
                            run.StartTime = result.StartTime;
                            // @TODO fix end time, does it matter for now?
                            run.EndTime = run.StartTime;
                        }
                        else
                        {
                            continue;
                        }
                    }
                } 
            }
        }

        private PerformanceTestResult TryDeserializePerformanceTestResultJsonObject(string json)
        {
            PerformanceTestResult performanceTestResult;
            try
            {
                performanceTestResult = JsonConvert.DeserializeObject<PerformanceTestResult>(json);
            }
            catch (Exception e)
            {
                var errMsg = string.Format("Exception thrown while deserializing json string to PerformanceTestResult: {0}", json);
                WriteExceptionConsoleErrorMessage(errMsg, e);
                throw;
            }

            return performanceTestResult;
        }

        private void WriteExceptionConsoleErrorMessage(string errMsg, Exception e)
        {
            Console.Error.WriteLine("{0}\r\nException: {1}\r\nInnerException: {2}", errMsg, e.Message,
                e.InnerException.Message);
        }

        private PerformanceTestRun TryDeserializePerformanceTestRunJsonObject(string json)
        {
            PerformanceTestRun performanceTestRun;
            try
            {
                performanceTestRun = JsonConvert.DeserializeObject<PerformanceTestRun>(json);
            }
            catch (Exception e)
            {
                var errMsg = string.Format("Exception thrown while deserializing json string to PerformanceTestRun: {0}", json);
                WriteExceptionConsoleErrorMessage(errMsg, e);
                throw;
            }

            return performanceTestRun;
        }

        public string GetJsonFromHashtag(string tag, string line)
        {
            if (!line.Contains($"##{tag}:")) return null;
            var jsonStart = line.IndexOf('{');
            var openBrackets = 0;
            var stringIndex = jsonStart;
            while (openBrackets > 0 || stringIndex == jsonStart)
            {
                var character = line[stringIndex];
                switch (character)
                {
                    case '{':
                        openBrackets++;
                        break;
                    case '}':
                        openBrackets--;
                        break;
                }

                stringIndex++;
            }
            var jsonEnd = stringIndex;
            return line.Substring(jsonStart, jsonEnd - jsonStart);
        }
    }
}
