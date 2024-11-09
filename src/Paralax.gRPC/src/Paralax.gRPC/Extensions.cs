using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Routing;
using Paralax.gRPC.Builders;
// using Paralax.gRPC.Protobuf;
// using Paralax.gRPC.Protobuf.Utilities;
// using Paralax.Health;
using System.Diagnostics;
using Paralax.gRPC.Utils;

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

                // builder.Services.AddCommonProtobufServices();
                builder.Services.AddSingleton<UptimeService>();
                builder.Services.AddSingleton<CpuUsageService>();
                builder.Services.AddSingleton<MemoryUsageService>();

                builder.Services.Configure<KestrelServerOptions>(kestrelOptions =>
                {
                    kestrelOptions.ListenAnyIP(options.RestPort, listenOptions =>
                    {
                        listenOptions.Protocols = HttpProtocols.Http1;
                    });
                    kestrelOptions.ListenAnyIP(options.GrpcPort, listenOptions =>
                    {
                        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
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

        // Extension for IApplicationBuilder to configure gRPC endpoints
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
                    // Enable gRPC reflection if configured
                    if (grpcOptions.EnableReflection)
                    {
                        endpoints.MapGrpcReflectionService();
                    }
                }
            });

            return app;
        }
    }
}
