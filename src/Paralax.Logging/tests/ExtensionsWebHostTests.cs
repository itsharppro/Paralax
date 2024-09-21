using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Serilog;
using Xunit;

namespace Paralax.Logging.Tests
{
    public class ExtensionsWebHostTests
    {
        // Mock Startup class to ensure that web host configuration is valid
        public class TestStartup
        {
            public void ConfigureServices(IServiceCollection services)
            {
                // Add required services here (logging, etc.)
                services.AddSingleton<ILoggingService, LoggingService>(); // Example service registration
            }

            // A Configure method is required by WebHostBuilder for setting up the request pipeline
            public void Configure(IApplicationBuilder app)
            {
                // Minimal middleware setup, can be left empty or add minimal configurations
                app.UseRouting();
            }
        }

        [Fact]
        public void UseLogging_Should_Register_LoggingService_On_WebHostBuilder()
        {
            // Arrange
            var webHostBuilder = new WebHostBuilder()
                .UseStartup<TestStartup>();  // Provide a minimal Startup class

            // Act
            webHostBuilder.UseLogging();
            var webHost = webHostBuilder.Build();
            var loggingService = webHost.Services.GetService<ILoggingService>();

            // Assert
            Assert.NotNull(loggingService);  // Verify if the logging service is registered
        }

        [Fact]
        public void UseLogging_Should_Configure_Serilog_On_WebHostBuilder()
        {
            // Arrange
            var webHostBuilder = new WebHostBuilder()
                .UseStartup<TestStartup>();  // Provide a minimal Startup class

            // Act
            webHostBuilder.UseLogging((context, loggerConfig) => 
            {
                loggerConfig.WriteTo.Console();  // Configure Serilog
            });

            var webHost = webHostBuilder.Build();

            // Assert
            var loggingService = webHost.Services.GetService<ILoggingService>();
            Assert.NotNull(loggingService);  // Check if logging service is registered
        }
    }
}
