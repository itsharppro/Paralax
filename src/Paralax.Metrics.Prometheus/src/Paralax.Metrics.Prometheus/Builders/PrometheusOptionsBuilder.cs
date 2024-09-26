using System.Collections.Generic;

namespace Paralax.Metrics.Prometheus.Builders
{
    public class PrometheusOptionsBuilder : IPrometheusOptionsBuilder
    {
        private readonly PrometheusOptions _options = new();

        public IPrometheusOptionsBuilder Enable(bool enabled)
        {
            _options.Enabled = enabled;
            return this;
        }

        public IPrometheusOptionsBuilder WithEndpoint(string endpoint)
        {
            _options.Endpoint = endpoint;
            return this;
        }

        public IPrometheusOptionsBuilder WithApiKey(string apiKey)
        {
            _options.ApiKey = apiKey;
            return this;
        }

        public IPrometheusOptionsBuilder WithAllowedHosts(IEnumerable<string> allowedHosts)
        {
            _options.AllowedHosts = allowedHosts;
            return this;
        }

        public PrometheusOptions Build()
        {
            return _options;
        }
    }
}
