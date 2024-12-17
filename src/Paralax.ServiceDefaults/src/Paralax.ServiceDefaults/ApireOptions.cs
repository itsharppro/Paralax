namespace Paralax.ServiceDefaults;

public class AspireOptions
{
    public string? Name { get; set; }
    public string? Version { get; set; }
    public bool DisplayVersion { get; set; } = true;
    public bool DisplayBanner { get; set; } = true;
    public string? BaseAddress { get; set; }
    public string? HealthCheckEndpoint { get; set; }
}