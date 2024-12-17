namespace Paralax.Diagnostics.HealthChecks;

public class HealthCheckConfiguration
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Endpoint { get; set; }
    public string HttpMethod { get; set; }
    public string ExpectedResponse { get; set; }
    public int Timeout { get; set; }
    public int Interval { get; set; }
    public int Retries { get; set; }
    public bool Enabled { get; set; }
}