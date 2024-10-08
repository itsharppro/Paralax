using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Routing;
using Paralax.gRPC.Builders;
using Paralax.gRPC.Protobuf;
using Paralax.gRPC.Protobuf.Utilities;
using Paralax.Health;
using Paralax.gRPC.Utils; 
using System.Diagnostics;

namespace Paralax.gRPC.Extensions
{
    public static class GrpcExtensions
    {
        public static IParalaxBuilder AddGrpc(this IParalaxBuilder builder, Action<GrpcOptionsBuilder> configureOptions)
        {
            var optionsBuilder = new GrpcOptionsBuilder();
            configureOptions?.Invoke(optionsBuilder);
            var grpcOptions = optionsBuilder.Build();

            builder.Services.AddSingleton(grpcOptions);

            builder.Services.AddCommonProtobufServices();

            builder.Services.AddSingleton<UptimeService>();
            builder.Services.AddSingleton<CpuUsageService>();
            builder.Services.AddSingleton<MemoryUsageService>();

            builder.Services.Configure<KestrelServerOptions>(options =>
            {
                options.ListenAnyIP(grpcOptions.Port, listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http2;
                });
            });

            builder.Services.AddGrpc(options =>
            {
                options.MaxReceiveMessageSize = grpcOptions.MaxReceiveMessageSize;
                options.MaxSendMessageSize = grpcOptions.MaxSendMessageSize;
            });

            return builder;
        }

        public static IApplicationBuilder UseGrpc(this IApplicationBuilder app, Action<IEndpointRouteBuilder> configureEndpoints)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var grpcOptions = scope.ServiceProvider.GetRequiredService<GrpcOptions>();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                configureEndpoints(endpoints);

                if (grpcOptions.EnableReflection)
                {
                    endpoints.MapGrpcReflectionService();
                }
            });

            return app;
        }

        public static HealthCheckResponse CreateHealthCheckResponse(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var grpcOptions = new GrpcOptions();
            configuration.GetSection("GrpcOptions").Bind(grpcOptions);

            var uptimeService = serviceProvider.GetRequiredService<UptimeService>();
            var cpuUsageService = serviceProvider.GetRequiredService<CpuUsageService>();
            var memoryUsageService = serviceProvider.GetRequiredService<MemoryUsageService>();

            var status = GrpcUtility.CreateSuccessStatus("Service is healthy.");

            return new HealthCheckResponse
            {
                Status = status,
                ServiceName = grpcOptions.ServiceName,
                ServiceVersion = grpcOptions.ServiceVersion,
                Environment = grpcOptions.Environment,
                UptimeSeconds = uptimeService.GetUptimeSeconds(),
                CpuUsagePercent = cpuUsageService.GetCpuUsage(),
                MemoryUsagePercent = memoryUsageService.GetMemoryUsage(),
                ActiveThreads = Process.GetCurrentProcess().Threads.Count,
                Metadata = MetadataUtility.CreateMetadata(grpcOptions.ServiceName)
            };
        }
    }
}
