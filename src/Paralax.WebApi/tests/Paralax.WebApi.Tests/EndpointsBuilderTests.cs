using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Paralax.WebApi;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

public class EndpointsBuilderIntegrationTests
{
    private readonly TestServer _testServer;
    private readonly HttpClient _httpClient;

    public EndpointsBuilderIntegrationTests()
    {
        var hostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddRouting(); // Add routing services
            })
            .Configure(app =>
            {
                // Ensure routing is used
                app.UseRouting();

                var definitions = new WebApiEndpointDefinitions();

                // Configure Endpoints using EndpointsBuilder within the routing scope
                app.UseEndpoints(endpoints =>
                {
                    var endpointsBuilder = new EndpointsBuilder(endpoints, definitions);

                    // Example endpoints registration
                    endpointsBuilder.Get("/test-get", async context =>
                    {
                        await context.Response.WriteAsync("GET Response");
                    });

                    endpointsBuilder.Post("/test-post", async context =>
                    {
                        await context.Response.WriteAsync("POST Response");
                    });

                    endpointsBuilder.Put("/test-put", async context =>
                    {
                        await context.Response.WriteAsync("PUT Response");
                    });

                    endpointsBuilder.Delete("/test-delete", async context =>
                    {
                        await context.Response.WriteAsync("DELETE Response");
                    });
                });
            });

        // Create TestServer and HttpClient
        _testServer = new TestServer(hostBuilder);
        _httpClient = _testServer.CreateClient();
    }

    [Fact]
    public async Task Get_Endpoint_Should_Return_Get_Response()
    {
        // Act
        var response = await _httpClient.GetAsync("/test-get");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        content.Should().Be("GET Response");
    }

    [Fact]
    public async Task Post_Endpoint_Should_Return_Post_Response()
    {
        // Act
        var response = await _httpClient.PostAsync("/test-post", null);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        content.Should().Be("POST Response");
    }

    [Fact]
    public async Task Put_Endpoint_Should_Return_Put_Response()
    {
        // Act
        var response = await _httpClient.PutAsync("/test-put", null);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        content.Should().Be("PUT Response");
    }

    [Fact]
    public async Task Delete_Endpoint_Should_Return_Delete_Response()
    {
        // Act
        var response = await _httpClient.DeleteAsync("/test-delete");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        content.Should().Be("DELETE Response");
    }
}
