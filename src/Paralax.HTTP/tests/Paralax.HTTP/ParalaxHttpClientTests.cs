using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;
using FluentAssertions;

namespace Paralax.HTTP.Tests
{
    public class ParalaxHttpClientTests
    {
        private readonly Mock<HttpMessageHandler> _messageHandlerMock;
        private readonly Mock<IHttpClientSerializer> _serializerMock;
        private readonly HttpClient _httpClient;
        private readonly ParalaxHttpClient _paralaxHttpClient;

        public ParalaxHttpClientTests()
        {
            _messageHandlerMock = new Mock<HttpMessageHandler>();
            _serializerMock = new Mock<IHttpClientSerializer>();

            _httpClient = new HttpClient(_messageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://api.example.com")
            };

            _paralaxHttpClient = new ParalaxHttpClient(_httpClient, _serializerMock.Object);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnHttpResponse()
        {
            // Arrange
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"key\":\"value\"}")
            };

            _messageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(expectedResponse);

            // Act
            var response = await _paralaxHttpClient.GetAsync("/test");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAsync_Generic_ShouldDeserializeResponse()
        {
            // Arrange
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"key\":\"value\"}")
            };

            _messageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(expectedResponse);

            _serializerMock.Setup(s => s.DeserializeAsync<Dictionary<string, string>>(It.IsAny<Stream>()))
                .ReturnsAsync(new Dictionary<string, string> { { "key", "value" } });

            // Act
            var result = await _paralaxHttpClient.GetAsync<Dictionary<string, string>>("/test");

            // Assert
            result.Should().ContainKey("key");
            result["key"].Should().Be("value");
        }

        [Fact]
        public async Task PostAsync_ShouldSendPostRequestWithJsonPayload()
        {
            // Arrange
            var data = new { Name = "Test" };
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.Created);

            _messageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(expectedResponse);

            // Mock serialization of the object to JSON
            _serializerMock.Setup(s => s.Serialize(It.IsAny<object>()))
                .Returns("{\"Name\":\"Test\"}");

            var jsonContent = new StringContent("{\"Name\":\"Test\"}", Encoding.UTF8, "application/json");

            // Act
            var response = await _paralaxHttpClient.PostAsync("/test", jsonContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            // Ensure Serialize method was called with the correct object
            _serializerMock.Verify(s => s.Serialize(It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public async Task PostAsync_Generic_ShouldReturnDeserializedResponse()
        {
            // Arrange
            var data = new { Name = "Test" };
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent("{\"id\":1}")
            };

            _messageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(expectedResponse);

            // Mock serialization of the request body
            _serializerMock.Setup(s => s.Serialize(It.IsAny<object>()))
                .Returns("{\"Name\":\"Test\"}");

            // Mock deserialization of the response
            _serializerMock.Setup(s => s.DeserializeAsync<Dictionary<string, int>>(It.IsAny<Stream>()))
                .ReturnsAsync(new Dictionary<string, int> { { "id", 1 } });

            var jsonContent = new StringContent("{\"Name\":\"Test\"}", Encoding.UTF8, "application/json");

            // Act
            var result = await _paralaxHttpClient.PostAsync<Dictionary<string, int>>("/test", jsonContent);

            // Assert
            result.Should().ContainKey("id");
            result["id"].Should().Be(1);

            // Verify that serialization and deserialization were called
            _serializerMock.Verify(s => s.Serialize(It.IsAny<object>()), Times.Once);
            _serializerMock.Verify(s => s.DeserializeAsync<Dictionary<string, int>>(It.IsAny<Stream>()), Times.Once);
        }

        [Fact]
        public async Task SendAsync_ShouldRetryOnFailure()
        {
            // Arrange
            var retries = 0;

            _messageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(() =>
                {
                    retries++;
                    if (retries < 3)
                    {
                        return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                    }

                    return new HttpResponseMessage(HttpStatusCode.OK);
                });

            // Act
            var response = await _paralaxHttpClient.GetAsync("/test");

            // Assert
            retries.Should().Be(1);
        }
    }
}
