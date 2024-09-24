using System;
using Paralax.gRPC.Utils;
using Xunit;

namespace Paralax.gRPC.Tests.Utils
{
    public class UptimeServiceTests
    {
        [Fact]
        public void GetUptimeSeconds_ShouldReturnNonNegativeValue()
        {
            // Arrange
            var uptimeService = new UptimeService();

            // Act
            var uptimeSeconds = uptimeService.GetUptimeSeconds();

            // Assert
            Assert.True(uptimeSeconds >= 0, "Uptime should not be negative.");
        }

        [Fact]
        public void GetUptimeSeconds_ShouldReturnReasonableValue()
        {
            // Arrange
            var uptimeService = new UptimeService();

            // Act
            var uptimeSeconds = uptimeService.GetUptimeSeconds();

            // Assert
            Assert.InRange(uptimeSeconds, 0, 604800);
        }

        [Fact]
        public void GetUptimeSeconds_ShouldNotThrowException()
        {
            // Arrange
            var uptimeService = new UptimeService();

            // Act and Assert
            var exception = Record.Exception(() => uptimeService.GetUptimeSeconds());
            Assert.Null(exception); 
        }

        [Fact]
        public void GetUptimeSeconds_ShouldIncreaseOverTime()
        {
            // Arrange
            var uptimeService = new UptimeService();

            // Act
            var uptime1 = uptimeService.GetUptimeSeconds();
            System.Threading.Thread.Sleep(1000); // Wait for 1 second
            var uptime2 = uptimeService.GetUptimeSeconds();

            // Assert
            Assert.True(uptime2 > uptime1, "Uptime should increase over time.");
        }
    }
}
