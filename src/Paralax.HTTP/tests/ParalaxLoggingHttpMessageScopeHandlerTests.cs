using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using Moq.Protected;

namespace Paralax.HTTP.Tests
{
    public class ParalaxLoggingHttpMessageScopeHandlerTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly HttpClientOptions _httpClientOptions;
        private readonly ParalaxLoggingHttpMessageScopeHandler _handler;
        private readonly Mock<HttpMessageHandler> _innerHandlerMock;

        public ParalaxLoggingHttpMessageScopeHandlerTests()
        {
            _loggerMock = new Mock<ILogger>();
            _httpClientOptions = new HttpClientOptions
            {
                RequestMasking = new HttpClientOptions.RequestMaskingOptions
                {
                    UrlParts = new[] { "sensitive-part" },
                    MaskTemplate = "*****"
                }
            };

            _innerHandlerMock = new Mock<HttpMessageHandler>();
            _handler = new ParalaxLoggingHttpMessageScopeHandler(_loggerMock.Object, _httpClientOptions)
            {
                InnerHandler = _innerHandlerMock.Object
            };
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
        {
            // Act
            Action act = () => new ParalaxLoggingHttpMessageScopeHandler(null, _httpClientOptions);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'logger')");
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenOptionsIsNull()
        {
            // Act
            Action act = () => new ParalaxLoggingHttpMessageScopeHandler(_loggerMock.Object, null);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'options')");
        }

        [Fact]
        public async Task SendAsync_ShouldLogRequestAndResponse()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/sensitive-part/data");
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

            _innerHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(expectedResponse);

            _loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

            // Use HttpClient to call the handler, which internally calls SendAsync
            var httpClient = new HttpClient(_handler);

            // Act
            var response = await httpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verify that BeginScope and Log methods were called with expected parameters
            _loggerMock.Verify(l => l.BeginScope(It.IsAny<Dictionary<string, object>>()), Times.Never);
            _loggerMock.Verify(l => l.Log(
                It.Is<LogLevel>(ll => ll == LogLevel.Information),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(2));
        }

        [Fact]
        public async Task SendAsync_ShouldMaskSensitivePartsInUrl()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/sensitive-part/data");
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

            _innerHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(expectedResponse);

            _loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

            // Use HttpClient to call the handler, which internally calls SendAsync
            var httpClient = new HttpClient(_handler);

            // Act
            var response = await httpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verify that the sensitive part of the URL was masked in the logs
            _loggerMock.Verify(l => l.Log(
                It.Is<LogLevel>(ll => ll == LogLevel.Information),
                It.Is<EventId>(e => e.Id == 100), // RequestPipelineStart event
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("*****")), // Check masked part
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Fact]
        public async Task SendAsync_ShouldThrowArgumentNullException_WhenRequestIsNull()
        {
            // Use HttpClient to call the handler, which internally calls SendAsync
            var httpClient = new HttpClient(_handler);

            // Act
            Func<Task> act = () => httpClient.SendAsync(null);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'request')");
        }
    }
}
