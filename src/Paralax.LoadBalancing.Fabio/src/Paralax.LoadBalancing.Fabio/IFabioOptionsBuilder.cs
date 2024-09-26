namespace Paralax.LoadBalancing.Fabio
{
    public interface IFabioOptionsBuilder
    {
        IFabioOptionsBuilder Enable(bool enabled);
        IFabioOptionsBuilder WithUrl(string url);
        IFabioOptionsBuilder WithService(string service);
        IFabioOptionsBuilder WithTimeout(int timeoutMilliseconds);
        IFabioOptionsBuilder WithRetries(int retryCount);
        IFabioOptionsBuilder EnableHealthCheck(bool enabled);
        IFabioOptionsBuilder WithHealthCheckPath(string healthCheckPath);
        IFabioOptionsBuilder WithCustomHeader(string key, string value);
        IFabioOptionsBuilder EnableCircuitBreaker(bool enabled);
        IFabioOptionsBuilder WithCircuitBreakerThreshold(int threshold);
        FabioOptions Build();
    }
}
