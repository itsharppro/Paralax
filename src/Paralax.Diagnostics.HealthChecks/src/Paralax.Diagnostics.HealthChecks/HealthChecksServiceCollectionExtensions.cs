using Microsoft.Extensions.DependencyInjection;

namespace Paralax.Diagnostics.HealthChecks;

public static class HealthChecksServiceCollectionExtensions
{
    public static IServiceCollection AddHealthChecks(this IServiceCollection services)
    {
        var registry = new HealthCheckRegistry();
        services.AddSingleton(registry);
        services.AddSingleton<HealthCheckService>();

        return services;
    }

    public static IServiceCollection AddCheck<THealthCheck>(this IServiceCollection services, string name, HealthStatus failureStatus = HealthStatus.Unhealthy)
        where THealthCheck : class, IHealthCheck
    {
        var registry = services.BuildServiceProvider().GetService<HealthCheckRegistry>();
        var healthCheckInstance = ActivatorUtilities.CreateInstance<THealthCheck>(services.BuildServiceProvider());
        registry?.Register(name, healthCheckInstance);

        return services;
    }
}
