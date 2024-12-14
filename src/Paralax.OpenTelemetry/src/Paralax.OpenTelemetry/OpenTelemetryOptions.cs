namespace Paralax.OpenTelemetry;

public class OpenTelemetryOptions
{
    public bool EnableTracing { get; set; } = true;
    public bool EnableMetrics { get; set; } = true;
    public bool EnableLogging { get; set; } = true;
    public string ServiceName { get; set; }
    public string? JaegerEndpoint { get; set; }
    public string? PrometheusEndpoint { get; set; }
}
