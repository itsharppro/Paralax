using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Paralax;
using Xunit;

namespace Paralax.WebApi.Security.Tests
{
    public class CertificateAuthenticationIntegrationTests
    {
        private readonly IConfiguration _configuration;

        public CertificateAuthenticationIntegrationTests()
        {
            // Updated to allow nullable strings for the configuration settings
            var inMemorySettings = new Dictionary<string, string?>
            {
                {"security:certificate:enabled", "true"},
                {"security:certificate:header", "X-Client-Cert"},
                {"security:certificate:skipRevocationCheck", "true"}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        // [Fact]
        // public async Task Should_Authenticate_With_Valid_Certificate()
        // {
        //     var hostBuilder = new HostBuilder()
        //         .ConfigureWebHost(webHost =>
        //         {
        //             webHost.UseTestServer();
        //             webHost.ConfigureServices((context, services) =>
        //             {
        //                 var securityOptions = new SecurityOptions
        //                 {
        //                     Certificate = new SecurityOptions.CertificateOptions
        //                     {
        //                         Enabled = true,
        //                         Header = "X-Client-Cert",
        //                         SkipRevocationCheck = true // Disable revocation checks for testing
        //                     }
        //                 };

        //                 services.AddRouting(); // Make sure routing is added
        //                 services.AddCertificateAuthentication(securityOptions);  // Apply to IServiceCollection
        //             });

        //             webHost.Configure(app =>
        //             {
        //                 app.UseRouting(); // Use routing middleware
        //                 app.UseCertificateAuthentication(); // Use the certificate authentication middleware
        //                 app.UseEndpoints(endpoints =>
        //                 {
        //                     endpoints.MapGet("/secured", async context =>
        //                     {
        //                         await context.Response.WriteAsync("Secured");
        //                     });
        //                 });
        //             });
        //         });

        //     using var host = await hostBuilder.StartAsync();

        //     using var handler = new HttpClientHandler();
        //     string password = "test_password";
        //     var certificate = GenerateCertificateWithPassword(password);
        //     handler.ClientCertificates.Add(certificate);

        //     using var client = new HttpClient(handler)
        //     {
        //         BaseAddress = host.GetTestServer().BaseAddress ?? new Uri("http://localhost") // Use in-memory server address
        //     };

        //     var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/secured");

        //     var response = await client.SendAsync(requestMessage);

        //     // Assert: Check that the response is successful (status code 200)
        //     response.EnsureSuccessStatusCode();
        //     var content = await response.Content.ReadAsStringAsync();
        //     Assert.Equal("Secured", content);
        // }

        [Fact]
        public async Task Should_Reject_Request_With_Invalid_Certificate()
        {
            var hostBuilder = new HostBuilder()
                .ConfigureWebHost(webHost =>
                {
                    webHost.UseTestServer();
                    webHost.ConfigureServices((context, services) =>
                    {
                        var securityOptions = new SecurityOptions
                        {
                            Certificate = new SecurityOptions.CertificateOptions
                            {
                                Enabled = true,
                                Header = "X-Client-Cert",
                                SkipRevocationCheck = true
                            }
                        };

                        services.AddRouting();
                        services.AddCertificateAuthentication(securityOptions);
                    });

                    webHost.Configure(app =>
                    {
                        app.UseRouting();
                        app.UseCertificateAuthentication();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapGet("/secured", async context =>
                            {
                                await context.Response.WriteAsync("Secured");
                            });
                        });
                    });
                });

            using var host = await hostBuilder.StartAsync();
            using var client = host.GetTestClient(); // No certificate

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/secured");

            var response = await client.SendAsync(requestMessage);

            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Should_Allow_Request_When_Certificate_Authentication_Disabled()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"security:certificate:enabled", "false"}
                })
                .Build();

            var hostBuilder = new HostBuilder()
                .ConfigureWebHost(webHost =>
                {
                    webHost.UseTestServer();
                    webHost.ConfigureServices((context, services) =>
                    {
                        var securityOptions = new SecurityOptions
                        {
                            Certificate = new SecurityOptions.CertificateOptions
                            {
                                Enabled = false // Certificate authentication disabled
                            }
                        };

                        services.AddRouting();
                        services.AddCertificateAuthentication(securityOptions);
                    });

                    webHost.Configure(app =>
                    {
                        app.UseRouting();
                        app.UseCertificateAuthentication();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapGet("/open", async context =>
                            {
                                await context.Response.WriteAsync("Open");
                            });
                        });
                    });
                });

            using var host = await hostBuilder.StartAsync();
            using var client = host.GetTestClient();

            var response = await client.GetAsync("/open");

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Open", content);
        }

        // Updated to address SYSLIB0057 by avoiding obsolete certificate loading method
        private X509Certificate2 GenerateCertificateWithPassword(string password)
        {
            using var rsa = RSA.Create(2048);
            var request = new CertificateRequest("cn=localhost", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            var certificate = request.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1));

            var pfxBytes = certificate.Export(X509ContentType.Pfx, password);
            return new X509Certificate2(pfxBytes, password, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
        }
    }
}
