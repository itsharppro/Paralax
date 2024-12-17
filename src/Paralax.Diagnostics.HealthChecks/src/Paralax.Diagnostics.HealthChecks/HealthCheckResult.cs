namespace Paralax.Diagnostics.HealthChecks;

public class HealthCheckResult
{
    public static HealthCheckResult Healthy(string? description = null, IReadOnlyDictionary<string, object>? data = null) => 
        new HealthCheckResult(HealthStatus.Healthy, description, data);

    public static HealthCheckResult Degraded(string? description = null, IReadOnlyDictionary<string, object>? data = null) => 
        new HealthCheckResult(HealthStatus.Degraded, description, data);

    public static HealthCheckResult Unhealthy(string? description = null, IReadOnlyDictionary<string, object>? data = null) => 
        new HealthCheckResult(HealthStatus.Unhealthy, description, data);

    public static HealthCheckResult Busy(string? description = null, IReadOnlyDictionary<string, object>? data = null) => 
        new HealthCheckResult(HealthStatus.Busy, description, data);

    public static HealthCheckResult Maintenance(string? description = null, IReadOnlyDictionary<string, object>? data = null) => 
        new HealthCheckResult(HealthStatus.Maintenance, description, data);

    public HealthStatus Status { get; }
    public string? Description { get; }
    public IReadOnlyDictionary<string, object>? Data { get; }

    private HealthCheckResult(HealthStatus status, string? description, IReadOnlyDictionary<string, object>? data)
    {
        Status = status;
        Description = description;
        Data = data ?? new Dictionary<string, object>();
    }
}
