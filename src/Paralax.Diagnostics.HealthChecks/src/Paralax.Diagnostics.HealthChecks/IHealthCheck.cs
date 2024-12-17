namespace Paralax.Diagnostics.HealthChecks;

public interface IHealthCheck
{
    Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default);
}