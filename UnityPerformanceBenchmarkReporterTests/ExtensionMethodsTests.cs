using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityPerformanceBenchmarkReporter;
using UnityPerformanceBenchmarkReporter.Entities;
using UnityPerformanceBenchmarkReporter.Report;

namespace UnityPerformanceBenchmarkReporterTests
{
    public class ExtensionMethodsTests
    {
        [Test]
        public void VerifyZeroValue()
        {
            // Arrange
            double d = 0;

            // Act
            var truncatedD = d.TruncToSigFig(2);

            // Assert
            Assert.AreEqual(0, truncatedD, 0);
        }
        
        [Test]
        public void VerifyNonZeroValue_GreaterThan1_0SigFigs()
        {
            // Arrange
            double d = 1.58888299999999993;

            // Act
            var truncatedD = d.TruncToSigFig(0);

            // Assert
            Assert.AreEqual(1, truncatedD, 0);
        }

        [Test]
        public void VerifyNonZeroValue_LessThan1_0SigFigs()
        {
            // Arrange
            double d = 0.58888299999999993;

            // Act
            var truncatedD = d.TruncToSigFig(0);

            // Assert
            Assert.AreEqual(0, truncatedD, 0);
        }

        [Test]
        public void VerifyNonZeroValue_GreaterThan1_1SigFigs()
        {
            // Arrange
            double d = 1.58888299999999993;

            // Act
            var truncatedD = d.TruncToSigFig(1);

            // Assert
            Assert.AreEqual(1.5, truncatedD, 0);
        }

        [Test]
        public void VerifyNonZeroValue_LessThan1_1SigFigs()
        {
            // Arrange
            double d = 0.58888299999999993;

            // Act
            var truncatedD = d.TruncToSigFig(1);

            // Assert
            Assert.AreEqual(0.5, truncatedD, 0);
        }
    }
}