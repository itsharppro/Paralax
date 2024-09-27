using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using System;
using Paralax.Tracing.Jaeger.Builders;

namespace Paralax.Tracing.Jaeger
{
    public static class Extensions
    {
        public static IParalaxBuilder AddJaegerTracing(this IParalaxBuilder builder)
        {
            // Get configuration and Jaeger options
            var configuration = builder.Services.BuildServiceProvider().GetRequiredService<IConfiguration>();
            var jaegerOptions = configuration.GetSection("Jaeger").Get<JaegerOptions>();

            if (!jaegerOptions.Enabled)
            {
                return builder;
            }

            // Add OpenTelemetry services
            builder.Services.AddOpenTelemetry()
                .WithTracing(traceBuilder =>
                {
                    traceBuilder
                        .SetResourceBuilder(ResourceBuilder.CreateDefault()
                            .AddService(jaegerOptions.ServiceName)) // Set service name
                        .AddAspNetCoreInstrumentation() // Add ASP.NET Core instrumentation
                        .AddHttpClientInstrumentation() // Add HTTP client instrumentation
                        .AddJaegerExporter(jaegerExporterOptions =>
                        {
                            // Configure Jaeger exporter settings
                            jaegerExporterOptions.Endpoint = new Uri(jaegerOptions.HttpSender?.Endpoint ?? "http://localhost:14268/api/traces");

                            // Note: OpenTelemetry Jaeger Exporter uses the Endpoint property for configuration.
                        });

                    if (!string.IsNullOrWhiteSpace(jaegerOptions.Sampler))
                    {
                        traceBuilder.SetSampler(new TraceIdRatioBasedSampler(jaegerOptions.SamplingRate));
                    }
                    else
                    {
                        traceBuilder.SetSampler(new AlwaysOnSampler());
                    }
                });

            return builder;
        }
    }
}
