using System;
using Paralax.gRPC.Utils;
using Xunit;

namespace Paralax.gRPC.Tests.Utils
{
    public class CpuUsageServiceTests
    {
        [Fact]
        public void GetCpuUsage_ShouldReturnUsageWithinValidRange()
        {
            // Arrange
            var cpuUsageService = new CpuUsageService();

            // Act
            var cpuUsage = cpuUsageService.GetCpuUsage();

            // Assert
            Assert.InRange(cpuUsage, 0, 100);
        }

        [Fact]
        public void GetCpuUsage_ShouldReturnNonNegativeValue()
        {
            // Arrange
            var cpuUsageService = new CpuUsageService();

            // Act
            var cpuUsage = cpuUsageService.GetCpuUsage();

            // Assert
            Assert.True(cpuUsage >= 0, "CPU usage should not be negative.");
        }

        [Fact]
        public void GetCpuUsage_ShouldNotThrowAnyException()
        {
            // Arrange
            var cpuUsageService = new CpuUsageService();

            // Act and Assert
            var exception = Record.Exception(() => cpuUsageService.GetCpuUsage());
            Assert.Null(exception); 
        }

        [Fact]
        public void GetCpuUsage_ShouldReturnReasonableValueWithMultipleCalls()
        {
            // Arrange
            var cpuUsageService = new CpuUsageService();

            // Act
            var firstUsage = cpuUsageService.GetCpuUsage();
            var secondUsage = cpuUsageService.GetCpuUsage();

            // Assert
            Assert.InRange(firstUsage, 0, 100);
            Assert.InRange(secondUsage, 0, 100);
        }
    }
}
