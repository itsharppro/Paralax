using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Paralax.Common;
using Paralax.gRPC.Protobuf.Utilities;
using Xunit;

namespace Paralax.gRPC.Tests.Utilities
{
    public class GrpcUtilityTests
    {
        private readonly Mock<ILogger> _mockLogger;

        public GrpcUtilityTests()
        {
            // Initialize the mock logger for testing
            _mockLogger = new Mock<ILogger>();
        }

        [Fact]
        public void CreateSuccessStatus_ShouldReturnSuccessStatus()
        {
            // Arrange
            var message = "Operation successful";

            // Act
            var result = GrpcUtility.CreateSuccessStatus(message);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(message, result.Message);
            Assert.Equal(200, result.Code);
            Assert.Equal("Operation completed successfully", result.Details);
        }

        [Fact]
        public void CreateErrorStatus_ShouldReturnErrorStatusAndLogError()
        {
            // Arrange
            var message = "An error occurred";
            var errorCode = 500;
            var details = "Detailed error message";

            // Act
            var result = GrpcUtility.CreateErrorStatus(message, errorCode, details, _mockLogger.Object);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(message, result.Message);
            Assert.Equal(errorCode, result.Code);
            Assert.Equal(details, result.Details);

            // Verify that the Log method was called with the expected log level and message
            _mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message) && v.ToString().Contains(details)),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        [Fact]
        public void CreateError_ShouldReturnErrorWithDetailsAndLogError()
        {
            // Arrange
            var code = 404;
            var message = "Resource not found";
            var details = new List<string> { "Detail1", "Detail2" };

            // Act
            var result = GrpcUtility.CreateError(code, message, details, _mockLogger.Object);

            // Assert
            Assert.Equal(code, result.Code);
            Assert.Equal(message, result.Message);
            Assert.Equal(details, result.Details);

            // Verify that the Log method was called with the expected log level and message
            _mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message) && v.ToString().Contains(string.Join(", ", details))),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        [Fact]
        public void CreateError_ShouldReturnErrorWithoutLogger()
        {
            // Arrange
            var code = 400;
            var message = "Bad request";
            var details = new List<string> { "Invalid input" };

            // Act
            var result = GrpcUtility.CreateError(code, message, details);

            // Assert
            Assert.Equal(code, result.Code);
            Assert.Equal(message, result.Message);
            Assert.Equal(details, result.Details);

            // Since logger is not provided, no logging should happen.
            _mockLogger.Verify(logger => logger.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }
    }
}
