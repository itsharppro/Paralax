namespace Paralax.Diagnostics.HealthChecks;

public class HealthCheckService
{
    private readonly HealthCheckRegistry _registry;

    public HealthCheckService(HealthCheckRegistry registry)
    {
        _registry = registry;
    }

    public async Task<Dictionary<string, HealthCheckResult>> RunHealthChecksAsync(CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<string, HealthCheckResult>();

        foreach (var healthCheck in _registry.HealthChecks)
        {
            var context = new HealthCheckContext { RegistrationName = healthCheck.Key };
            results[healthCheck.Key] = await healthCheck.Value.CheckHealthAsync(context, cancellationToken).ConfigureAwait(false);
        }

        return results;
    }
}