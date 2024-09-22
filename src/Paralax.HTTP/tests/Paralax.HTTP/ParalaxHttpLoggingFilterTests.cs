using System;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace Paralax.HTTP.Tests
{
    public class ParalaxHttpLoggingFilterTests
    {
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly HttpClientOptions _httpClientOptions;

        public ParalaxHttpLoggingFilterTests()
        {
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerMock = new Mock<ILogger>();
            _httpClientOptions = new HttpClientOptions();

            // Setup the logger factory to return a mocked logger
            _loggerFactoryMock.Setup(lf => lf.CreateLogger(It.IsAny<string>()))
                .Returns(_loggerMock.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenLoggerFactoryIsNull()
        {
            // Act
            Action act = () => new ParalaxHttpLoggingFilter(null, _httpClientOptions);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'loggerFactory')");
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenHttpClientOptionsIsNull()
        {
            // Act
            Action act = () => new ParalaxHttpLoggingFilter(_loggerFactoryMock.Object, null);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'options')");
        }

        [Fact]
        public void Configure_ShouldThrowArgumentNullException_WhenNextIsNull()
        {
            // Arrange
            var filter = new ParalaxHttpLoggingFilter(_loggerFactoryMock.Object, _httpClientOptions);

            // Act
            Action act = () => filter.Configure(null);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'next')");
        }

        [Fact]
        public void Configure_ShouldInsertLoggingHandlerAtBeginning()
        {
            // Arrange
            var filter = new ParalaxHttpLoggingFilter(_loggerFactoryMock.Object, _httpClientOptions);
            var builderMock = new Mock<HttpMessageHandlerBuilder>();
            var additionalHandlers = new System.Collections.Generic.List<DelegatingHandler>();
            builderMock.SetupGet(b => b.AdditionalHandlers).Returns(additionalHandlers);
            builderMock.SetupGet(b => b.Name).Returns("TestClient");

            Action<HttpMessageHandlerBuilder> next = _ => { /* next action logic */ };

            // Act
            var configuredAction = filter.Configure(next);
            configuredAction(builderMock.Object);

            // Assert
            _loggerFactoryMock.Verify(lf => lf.CreateLogger(It.Is<string>(s => s == "System.Net.Http.HttpClient.TestClient.LogicalHandler")), Times.Once);

            // Ensure the custom logging handler is inserted at the beginning
            additionalHandlers.Should().HaveCount(1);
            additionalHandlers[0].Should().BeOfType<ParalaxLoggingHttpMessageScopeHandler>();
        }
    }
}
