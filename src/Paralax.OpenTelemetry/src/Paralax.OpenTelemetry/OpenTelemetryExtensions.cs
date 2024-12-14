using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using OpenTelemetry;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Microsoft.AspNetCore.Builder;

using Paralax.OpenTelemetry.Builders;

namespace Paralax.OpenTelemetry;

public static class OpenTelemetryExtensions
{
    private const string OpenTelemetryOptionsSection = "OpenTelemetry";

    public static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
    {
        var options = builder.GetOptions<OpenTelemetryOptions>(OpenTelemetryOptionsSection) ?? new OpenTelemetryOptions();

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        if (options.EnableMetrics || options.EnableTracing || options.EnableLogging)
        {
            var openTelemetryBuilder = builder.Services.AddOpenTelemetry();

            if (options.EnableMetrics)
            {
                openTelemetryBuilder.WithMetrics(metrics =>
                {
                    metrics.AddAspNetCoreInstrumentation()
                           .AddHttpClientInstrumentation()
                           .AddRuntimeInstrumentation();

                    if (!string.IsNullOrWhiteSpace(options.PrometheusEndpoint))
                    {
                        // Metrics provides by ASP.NET Core in .NET 8
                         metrics.AddMeter("Microsoft.AspNetCore.Hosting")
                                .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                                .AddPrometheusExporter();
                    }
                });
            }

            if (options.EnableTracing)
            {
                openTelemetryBuilder.WithTracing(tracing =>
                {
                    tracing.AddSource(options.ServiceName ?? builder.Environment.ApplicationName)
                           .AddAspNetCoreInstrumentation(tracingOptions =>
                           {
                               tracingOptions.Filter = httpContext =>
                                   !(httpContext.Request.Path.StartsWithSegments("/health")
                                   || httpContext.Request.Path.StartsWithSegments("/alive"));
                           })
                           .AddGrpcClientInstrumentation()
                           .AddHttpClientInstrumentation();

                    if (!string.IsNullOrWhiteSpace(options.JaegerEndpoint))
                    {
                        tracing.AddJaegerExporter(jaegerOptions =>
                        {
                            jaegerOptions.AgentHost = options.JaegerEndpoint;
                        });
                    }
                });
            }
        }

        builder.AddOpenTelemetryExporters(options);

        return builder;
    }

    private static IHostApplicationBuilder AddOpenTelemetryExporters(this IHostApplicationBuilder builder, OpenTelemetryOptions options)
    {
        if (!string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        return builder;
    }

    public static IHostApplicationBuilder AddDefaultHealthChecks(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), new[] { "live" });

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapHealthChecks("/health");

            app.MapHealthChecks("/alive", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });
        }

        return app;
    }

    public static T? GetOptions<T>(this IHostApplicationBuilder builder, string sectionName) where T : class, new()
    {
        var section = builder.Configuration.GetSection(sectionName);
        if (!section.Exists()) return null;

        var options = new T();
        section.Bind(options);
        return options;
    }
}
