using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Paralax.Logging;
using Moq;
using Xunit;

namespace Paralax.Logging.Tests
{
    public class CorrelationContextLoggingMiddlewareTests
    {
        private readonly Mock<ILogger<CorrelationContextLoggingMiddleware>> _loggerMock;
        private readonly Mock<RequestDelegate> _nextMock;
        private readonly CorrelationContextLoggingMiddleware _middleware;

        public CorrelationContextLoggingMiddlewareTests()
        {
            // Set up the mocks
            _loggerMock = new Mock<ILogger<CorrelationContextLoggingMiddleware>>();
            _nextMock = new Mock<RequestDelegate>();

            // Instantiate the middleware with the mock logger
            _middleware = new CorrelationContextLoggingMiddleware(_loggerMock.Object);
        }

        [Fact]
        public async Task InvokeAsync_Should_Log_Correlation_Headers_When_Activity_Exists()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var baggage = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("CorrelationId", "12345"),
                new KeyValuePair<string, string>("UserId", "User123")
            };

            // Simulate an Activity with baggage
            var activity = new Activity("TestActivity");
            activity.AddBaggage("CorrelationId", "12345");
            activity.AddBaggage("UserId", "User123");
            activity.Start();

            _nextMock.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(httpContext, _nextMock.Object);

            // Assert
            _loggerMock.Verify(logger => 
                logger.BeginScope(It.Is<Dictionary<string, string>>(d =>
                    d.ContainsKey("CorrelationId") && d["CorrelationId"] == "12345" &&
                    d.ContainsKey("UserId") && d["UserId"] == "User123")), Times.Once);

            // Cleanup
            activity.Stop();
        }

        [Fact]
        public async Task InvokeAsync_Should_Log_Empty_Headers_When_Activity_Is_Null()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();

            // No active activity, meaning Activity.Current is null
            Activity.Current = null;

            _nextMock.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(httpContext, _nextMock.Object);

            // Assert
            _loggerMock.Verify(logger =>
                logger.BeginScope(It.Is<Dictionary<string, string>>(d => d.Count == 0)), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_Should_Call_Next_Middleware()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();

            // Simulate an empty Activity
            var activity = new Activity("TestActivity").Start();
            _nextMock.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(httpContext, _nextMock.Object);

            // Assert
            _nextMock.Verify(next => next(It.IsAny<HttpContext>()), Times.Once);

            // Cleanup
            activity.Stop();
        }
    }
}
