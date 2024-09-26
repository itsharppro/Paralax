using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using System;

namespace Paralax.Tracing.Jaeger.Tracers
{
    public sealed class ParalaxDefaultTracers
    {
        public static void AddTracing(IServiceCollection services, JaegerOptions options)
        {
            if (!options.Enabled)
            {
                return;
            }

            services.AddOpenTelemetry()
                .WithTracing(builder =>
                {
                    builder
                        .SetResourceBuilder(ResourceBuilder.CreateDefault()
                            .AddService(options.ServiceName)) // Adds service name to resource attributes
                        .AddAspNetCoreInstrumentation()  // Adds instrumentation for ASP.NET Core requests
                        .AddHttpClientInstrumentation()  // Adds instrumentation for outgoing HTTP client calls
                        .AddJaegerExporter(jaegerOptions =>
                        {
                            jaegerOptions.AgentHost = options.UdpHost ?? "localhost";
                            jaegerOptions.AgentPort = options.UdpPort > 0 ? options.UdpPort : 6831;
                            jaegerOptions.MaxPayloadSizeInBytes = options.MaxPacketSize;

                            // If HTTP sender is configured, set the HTTP endpoint
                            if (options.HttpSender != null && !string.IsNullOrEmpty(options.HttpSender.Endpoint))
                            {
                                jaegerOptions.Endpoint = new Uri(options.HttpSender.Endpoint);
                            }
                        });

                    if (!string.IsNullOrWhiteSpace(options.Sampler))
                    {
                        builder.SetSampler(new TraceIdRatioBasedSampler(options.SamplingRate));
                    }
                    else
                    {
                        builder.SetSampler(new AlwaysOnSampler());
                    }
                });
        }
    }
}
