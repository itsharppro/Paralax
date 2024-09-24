using System;
using Paralax.gRPC.Utils;
using Xunit;

namespace Paralax.gRPC.Tests.Utils
{
    public class MemoryUsageServiceTests
    {
        [Fact]
        public void GetMemoryUsage_ShouldReturnNonNegativeValue()
        {
            // Arrange
            var memoryUsageService = new MemoryUsageService();

            // Act
            var memoryUsage = memoryUsageService.GetMemoryUsage();

            // Assert
            Assert.True(memoryUsage >= 0, "Memory usage should not be negative.");
        }

        [Fact]
        public void GetMemoryUsage_ShouldReturnMemoryUsageInMB()
        {
            // Arrange
            var memoryUsageService = new MemoryUsageService();

            // Act
            var memoryUsage = memoryUsageService.GetMemoryUsage();

            // Assert
            Assert.True(memoryUsage >= 0, "Memory usage should be non-negative.");
            Assert.InRange(memoryUsage, 0, double.MaxValue);
        }

        [Fact]
        public void GetMemoryUsage_ShouldNotThrowException()
        {
            // Arrange
            var memoryUsageService = new MemoryUsageService();

            // Act and Assert
            var exception = Record.Exception(() => memoryUsageService.GetMemoryUsage());
            Assert.Null(exception); 
        }

        [Fact]
        public void GetMemoryUsage_ShouldReturnReasonableValue()
        {
            // Arrange
            var memoryUsageService = new MemoryUsageService();

            // Act
            var memoryUsage = memoryUsageService.GetMemoryUsage();

            // Assert
            Assert.True(memoryUsage > 0, "Memory usage should be greater than 0 MB.");
        }
    }
}
