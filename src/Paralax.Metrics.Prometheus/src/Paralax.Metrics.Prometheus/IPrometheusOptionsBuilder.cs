using System.Collections.Generic;

namespace Paralax.Metrics.Prometheus
{
    public interface IPrometheusOptionsBuilder
    {
        IPrometheusOptionsBuilder Enable(bool enabled);
        IPrometheusOptionsBuilder WithEndpoint(string endpoint);
        IPrometheusOptionsBuilder WithApiKey(string apiKey);
        IPrometheusOptionsBuilder WithAllowedHosts(IEnumerable<string> allowedHosts);
        PrometheusOptions Build();
    }
}