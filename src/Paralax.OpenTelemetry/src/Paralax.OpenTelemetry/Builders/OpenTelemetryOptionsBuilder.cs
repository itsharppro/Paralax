namespace Paralax.OpenTelemetry.Builders;

public class OpenTelemetryOptionsBuilder
{
    private readonly OpenTelemetryOptions _options = new();

    public OpenTelemetryOptionsBuilder EnableTracing(bool enable)
    {
        _options.EnableTracing = enable;
        return this;
    }

    public OpenTelemetryOptionsBuilder EnableMetrics(bool enable)
    {
        _options.EnableMetrics = enable;
        return this;
    }

    public OpenTelemetryOptionsBuilder EnableLogging(bool enable)
    {
        _options.EnableLogging = enable;
        return this;
    }

    public OpenTelemetryOptionsBuilder SetServiceName(string serviceName)
    {
        _options.ServiceName = serviceName;
        return this;
    }

    public OpenTelemetryOptionsBuilder SetJaegerEndpoint(string endpoint)
    {
        _options.JaegerEndpoint = endpoint;
        return this;
    }

    public OpenTelemetryOptionsBuilder SetPrometheusEndpoint(string endpoint)
    {
        _options.PrometheusEndpoint = endpoint;
        return this;
    }

    public OpenTelemetryOptions Build()
    {
        return _options;
    }
}