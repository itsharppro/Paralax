using System.Collections.Concurrent;

namespace Paralax.Diagnostics.HealthChecks;

public class HealthCheckRegistry
{
    private readonly ConcurrentDictionary<string, IHealthCheck> _healthChecks = new();

    public bool Register(string name, IHealthCheck healthCheck)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Health check name must not be null or whitespace.", nameof(name));
        
        return _healthChecks.TryAdd(name, healthCheck);
    }

    public bool Unregister(string name)
    {
        return _healthChecks.TryRemove(name, out var _);
    }

    public IEnumerable<string> RegisteredChecks => _healthChecks.Keys;

    internal ConcurrentDictionary<string, IHealthCheck> HealthChecks => _healthChecks;
}

