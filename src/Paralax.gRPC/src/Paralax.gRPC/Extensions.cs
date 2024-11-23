using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Paralax.gRPC.Builders;

namespace Paralax.gRPC
{
    public static class Extensions
    {
        private const string GrpcOptionsSection = "GrpcOptions";

        public static IParalaxBuilder AddParalaxGrpc(this IParalaxBuilder builder, string sectionName = GrpcOptionsSection, Action<GrpcOptionsBuilder>? configureOptions = null)
        {
            var optionsList = builder.GetOptions<List<GrpcOptions>>(sectionName) ?? new List<GrpcOptions>
            {
                new GrpcOptions
                {
                    RestPort = 5045,
                    GrpcPort = 7146,
                    EnableReflection = true,
                    MaxReceiveMessageSize = 8 * 1024 * 1024,
                    MaxSendMessageSize = 8 * 1024 * 1024,
                    ServiceName = "DefaultService",
                    ServiceVersion = "1.0.0",
                    Environment = "Production"
                }
            };

            foreach (var options in optionsList)
            {
                builder.Services.AddSingleton(options);

                configureOptions?.Invoke(new GrpcOptionsBuilder());

                builder.Services.Configure<KestrelServerOptions>(kestrelOptions =>
                {
                    // Configure REST endpoint (HTTP/1.1)
                    kestrelOptions.ListenAnyIP(options.RestPort, listenOptions =>
                    {
                        listenOptions.Protocols = HttpProtocols.Http1;
                    });

                    // Configure gRPC endpoint (HTTP/1.1 and HTTP/2)
                    kestrelOptions.ListenAnyIP(options.GrpcPort, listenOptions =>
                    {
                        if (options.UseHttps)
                        {
                            if (string.IsNullOrEmpty(options.PemCertificatePath) || string.IsNullOrEmpty(options.KeyCertificatePath))
                            {
                                throw new InvalidOperationException("TLS is enabled, but certificate paths are not provided.");
                            }

                            // Load the certificate from PEM and Key files
                            var certificate = LoadCertificate(options.PemCertificatePath, options.KeyCertificatePath);

                            listenOptions.UseHttps(certificate);
                        }

                        listenOptions.Protocols = HttpProtocols.Http1AndHttp2; // Support both HTTP/1.1 and HTTP/2
                    });
                });

                builder.Services.AddGrpc(grpcOptions =>
                {
                    grpcOptions.MaxReceiveMessageSize = options.MaxReceiveMessageSize;
                    grpcOptions.MaxSendMessageSize = options.MaxSendMessageSize;
                });

                if (options.EnableReflection)
                {
                    builder.Services.AddGrpcReflection();
                }
            }

            return builder;
        }

        public static IApplicationBuilder UseParalaxGrpc(this IApplicationBuilder app, Action<IEndpointRouteBuilder>? configureEndpoints = null)
        {
            var grpcOptionsList = app.ApplicationServices.GetServices<GrpcOptions>();

            if (grpcOptionsList == null || !grpcOptionsList.Any())
            {
                throw new InvalidOperationException("No GrpcOptions have been registered.");
            }

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                configureEndpoints?.Invoke(endpoints);

                foreach (var grpcOptions in grpcOptionsList)
                {
                    if (grpcOptions.EnableReflection)
                    {
                        endpoints.MapGrpcReflectionService();
                    }
                }
            });

            return app;
        }

        private static X509Certificate2 LoadCertificate(string pemPath, string keyPath)
        {
            var certPem = System.IO.File.ReadAllText(pemPath);
            var keyPem = System.IO.File.ReadAllText(keyPath);

            // Load the certificate with the private key from PEM and key files
            return X509Certificate2.CreateFromPem(certPem, keyPem);
        }

    }
}
