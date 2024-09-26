using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus.DotNetRuntime;

namespace Paralax.Metrics.Prometheus.Core
{
    internal sealed class PrometheusJob : IHostedService
    {
        private IDisposable _collector;
        private readonly ILogger<PrometheusJob> _logger;
        private readonly bool _enabled;

        public PrometheusJob(PrometheusOptions options, ILogger<PrometheusJob> logger)
        {
            _enabled = options.Enabled;
            _logger = logger;
            _logger.LogInformation($"Prometheus integration is {(_enabled ? "enabled" : "disabled")}.");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_enabled)
            {
                _collector = DotNetRuntimeStatsBuilder
                    .Customize()
                    .WithContentionStats()    // Collect stats on lock contention
                    .WithJitStats()           // Collect stats on JIT compilation
                    .WithThreadPoolStats()    // Collect stats on thread pool usage
                    .WithGcStats()            // Collect stats on garbage collection
                    .WithExceptionStats()     // Collect stats on thrown exceptions
                    .StartCollecting();
                
                _logger.LogInformation("Started collecting Prometheus .NET runtime metrics.");
            }
            else
            {
                _logger.LogInformation("Prometheus metrics collection is disabled.");
            }
            
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _collector?.Dispose();
            _logger.LogInformation("Stopped collecting Prometheus .NET runtime metrics.");

            return Task.CompletedTask;
        }
    }
}
