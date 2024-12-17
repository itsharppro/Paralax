namespace Paralax.Diagnostics.HealthChecks;

public abstract class HealthCheckBase : IHealthCheck
{
    public abstract Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, 
            CancellationToken cancellationToken = default);
}
