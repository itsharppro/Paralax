using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Paralax.Metrics.Prometheus.Core;
using Prometheus;
using Prometheus.SystemMetrics;

namespace Paralax.Metrics.Prometheus
{
    public static class Extensions
    {
        /// <summary>
        /// Adds Prometheus metrics and related services to the application.
        /// </summary>
        public static IServiceCollection AddPrometheus(this IServiceCollection services, PrometheusOptions options)
        {
            services.AddSingleton(options);

            if (!options.Enabled)
            {
                return services;
            }

            services.AddHostedService<PrometheusJob>();
            services.AddTransient<PrometheusMiddleware>();
            services.AddSystemMetrics();

            return services;
        }

        /// <summary>
        /// Configures the application to use Prometheus metrics collection.
        /// </summary>
        public static IApplicationBuilder UsePrometheus(this IApplicationBuilder app)
        {
            var options = app.ApplicationServices.GetRequiredService<PrometheusOptions>();

            if (!options.Enabled)
            {
                return app;
            }

            var endpoint = string.IsNullOrWhiteSpace(options.Endpoint) ? "/metrics" :
                options.Endpoint.StartsWith("/") ? options.Endpoint : $"/{options.Endpoint}";

            return app
                .UseMiddleware<PrometheusMiddleware>()
                .UseHttpMetrics()       // Collect HTTP metrics
                .UseGrpcMetrics()       // Collect gRPC metrics
                .UseMetricServer(endpoint);  // Expose the metrics on the defined endpoint
        }
    }
}
