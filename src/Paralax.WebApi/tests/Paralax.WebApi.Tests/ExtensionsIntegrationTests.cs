using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Paralax.WebApi;
using Newtonsoft.Json;
using Paralax.WebApi.Requests;
using Paralax.WebApi.Exceptions;
using System.Net;

public class ExtensionsIntegrationTests
{
    private readonly TestServer _testServer;
    private readonly HttpClient _httpClient;

    public ExtensionsIntegrationTests()
    {
        var hostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddRouting();
                services.AddLogging();
                services.AddScoped<IRequestHandler<TestRequest, TestResponse>, TestRequestHandler>();
                
                // Register ErrorHandlerMiddleware and the IExceptionToResponseMapper dependency
                services.AddTransient<ErrorHandlerMiddleware>();
                services.AddTransient<IExceptionToResponseMapper, ExceptionToResponseMapper>();
            })
            .Configure(app =>
            {
                app.UseRouting();

                // Use the ErrorHandler middleware from the Extensions class
                app.UseErrorHandler();

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapPost("/dispatch", async context =>
                    {
                        var request = await context.ReadJsonAsync<TestRequest>();
                        var response = await context.DispatchAsync<TestRequest, TestResponse>(request);
                        await context.Response.Ok(response);
                    });

                    endpoints.MapGet("/test-ok", async context =>
                    {
                        var response = new TestResponse { Message = "Ok Response" };
                        await context.Response.Ok(response);
                    });

                    endpoints.MapPost("/test-invalid-json", async context =>
                    {
                        await context.ReadJsonAsync<TestRequest>(); // Should handle invalid JSON
                    });
                });
            });

        // Create the TestServer and HttpClient
        _testServer = new TestServer(hostBuilder);
        _httpClient = _testServer.CreateClient();
    }

    [Fact]
    public async Task Ok_ShouldReturnOkStatus_WithJsonResponse()
    {
        // Act
        var response = await _httpClient.GetAsync("/test-ok");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        var responseObject = JsonConvert.DeserializeObject<TestResponse>(content);
        responseObject.Should();
    }

    [Fact]
    public async Task DispatchAsync_ShouldHandleRequestAndReturnResponse()
    {
        // Arrange
        var request = new TestRequest { Name = "Test User" };
        var jsonRequest = JsonConvert.SerializeObject(request);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        // Act
        var response = await _httpClient.PostAsync("/dispatch", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        var responseObject = JsonConvert.DeserializeObject<TestResponse>(responseContent);
        responseObject.Should();
    }

    [Fact]
    public async Task ReadJsonAsync_ShouldReturnBadRequest_OnInvalidJson()
    {
        // Arrange
        var invalidJson = " Invalid }"; // Malformed JSON
        var content = new StringContent(invalidJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _httpClient.PostAsync("/test-invalid-json", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Mock Request and Response
    public class TestRequest : IRequest
    {
        public string Name { get; set; }
    }

    public class TestResponse
    {
        public string Message { get; set; }
    }

    public class TestRequestHandler : IRequestHandler<TestRequest, TestResponse>
    {
        public Task<TestResponse> HandleAsync(TestRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new TestResponse { Message = $"Hello, {request.Name}" });
        }
    }

    // Mock ExceptionToResponseMapper
    public class ExceptionToResponseMapper : IExceptionToResponseMapper
    {
        public ExceptionResponse Map(Exception exception)
        {
            return new ExceptionResponse(new { Message = "Error occurred" }, HttpStatusCode.BadRequest);
        }
    }
}
