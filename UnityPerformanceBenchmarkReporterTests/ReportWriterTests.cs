using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityPerformanceBenchmarkReporter.Entities;
using UnityPerformanceBenchmarkReporter.Report;

namespace UnityPerformanceBenchmarkReporterTests
{
    public class ReportWriterTests
    {
        private ReportWriter tr;

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void WhenResultsAreNull_ReporterWriterThrowsArgNullException()
        {
            // Arrange
            tr = new ReportWriter();

            // Act/Assert
            Assert.Throws<ArgumentNullException>(() => tr.WriteReport(null));
        }

        [Test]
        public void WhenResultsAreEmpty_ReporterWriterThrowsArgNullException()
        {
            // Arrange
            tr = new ReportWriter();
            var results = new List<PerformanceTestRunResult>();

            // Act/Assert
            Assert.Throws<ArgumentNullException>(() => tr.WriteReport(results));
        }
    }
}