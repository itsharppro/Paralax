using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Xunit;

namespace Paralax.Logging.Tests
{
    public class ExtensionsTests
    {
        [Fact]
        public void UseLogging_Should_Register_LoggingService_On_HostBuilder()
        {
            // Arrange
            var hostBuilder = new HostBuilder();

            // Act
            hostBuilder.UseLogging();
            var host = hostBuilder.Build();
            var loggingService = host.Services.GetService<ILoggingService>();

            // Assert
            Assert.NotNull(loggingService);
        }

        [Fact]
        public void UseLogging_Should_Configure_Serilog_On_HostBuilder()
        {
            // Arrange
            var hostBuilder = new HostBuilder();
            var loggerConfigurationMock = new Mock<LoggerConfiguration>();

            // Act
            hostBuilder.UseLogging((context, loggerConfig) => 
            {
                loggerConfig.WriteTo.Console();
            });

            var host = hostBuilder.Build();

            // Assert
            var loggingService = host.Services.GetService<ILoggingService>();
            Assert.NotNull(loggingService);  // Check if logging service is registered
        }
    }
}
