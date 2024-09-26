using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using System;
using Paralax.Tracing.Jaeger.Builders;

namespace Paralax.Tracing.Jaeger
{
    public static class Extensions
    {
        // Initializes Jaeger tracing with OpenTelemetry and configures the service collection
        public static IServiceCollection AddJaegerTracing(this IServiceCollection services, Action<JaegerOptionsBuilder> configureOptions)
        {
            var builder = new JaegerOptionsBuilder();
            configureOptions(builder);
            var options = builder.Build();

            if (!options.Enabled)
            {
                return services; // Return early if tracing is not enabled
            }

            services.AddOpenTelemetry()
                .WithTracing(traceBuilder =>
                {
                    traceBuilder
                        .SetResourceBuilder(ResourceBuilder.CreateDefault()
                            .AddService(options.ServiceName)) // Set service name
                        .AddAspNetCoreInstrumentation() // Add ASP.NET Core instrumentation
                        .AddHttpClientInstrumentation() // Add HTTP client instrumentation
                        .AddJaegerExporter(jaegerOptions =>
                        {
                            // Configure Jaeger exporter settings
                            jaegerOptions.AgentHost = options.UdpHost ?? "localhost";
                            jaegerOptions.AgentPort = options.UdpPort > 0 ? options.UdpPort : 6831;
                            jaegerOptions.MaxPayloadSizeInBytes = options.MaxPacketSize;

                            if (options.HttpSender != null && !string.IsNullOrEmpty(options.HttpSender.Endpoint))
                            {
                                jaegerOptions.Endpoint = new Uri(options.HttpSender.Endpoint);
                            }
                        });

                    if (!string.IsNullOrWhiteSpace(options.Sampler))
                    {
                        traceBuilder.SetSampler(new TraceIdRatioBasedSampler(options.SamplingRate));
                    }
                    else
                    {
                        traceBuilder.SetSampler(new AlwaysOnSampler());
                    }
                });

            return services;
        }

        public static IApplicationBuilder UseJaegerTracing(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var options = scope.ServiceProvider.GetRequiredService<JaegerOptions>();

            if (!options.Enabled)
            {
                return app; 
            }

            return app;
        }
    }
}
