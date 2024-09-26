namespace Paralax.LoadBalancing.Fabio.Builders
{
    public class FabioOptionsBuilder : IFabioOptionsBuilder
    {
        private readonly FabioOptions _options = new();

        public IFabioOptionsBuilder Enable(bool enabled)
        {
            _options.Enabled = enabled;
            return this;
        }

        public IFabioOptionsBuilder WithUrl(string url)
        {
            _options.Url = url;
            return this;
        }

        public IFabioOptionsBuilder WithService(string service)
        {
            _options.Service = service;
            return this;
        }

        public IFabioOptionsBuilder WithTimeout(int timeoutMilliseconds)
        {
            _options.TimeoutMilliseconds = timeoutMilliseconds;
            return this;
        }

        public IFabioOptionsBuilder WithRetries(int retryCount)
        {
            _options.RetryCount = retryCount;
            return this;
        }

        public IFabioOptionsBuilder EnableHealthCheck(bool enabled)
        {
            _options.HealthCheckEnabled = enabled;
            return this;
        }

        public IFabioOptionsBuilder WithHealthCheckPath(string healthCheckPath)
        {
            _options.HealthCheckPath = healthCheckPath;
            return this;
        }

        public IFabioOptionsBuilder WithCustomHeader(string key, string value)
        {
            _options.CustomHeaders[key] = value;
            return this;
        }

        public IFabioOptionsBuilder EnableCircuitBreaker(bool enabled)
        {
            _options.CircuitBreakerEnabled = enabled;
            return this;
        }

        public IFabioOptionsBuilder WithCircuitBreakerThreshold(int threshold)
        {
            _options.CircuitBreakerThreshold = threshold;
            return this;
        }

        public FabioOptions Build()
        {
            return _options;
        }
    }
}
