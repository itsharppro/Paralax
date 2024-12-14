using Paralax.Net.Http;
using Paralax.Grpc;
using Paralax.Diagnostics.HealthChecks;
using Paralax.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Grpc.Net.Client;

namespace Paralax.ServiceDefaults;

public static class ServiceReferenceExtensions
{
    public static IServiceCollection AddHttpServiceReference<TClient>(this IServiceCollection services, AspireOptions options)
        where TClient : class
    {
        if (!Uri.IsWellFormedUriString(options.BaseAddress, UriKind.Absolute))
            throw new ArgumentException("Base address must be a valid absolute URI.", nameof(options.BaseAddress));

        services.AddHttpClient<TClient>(client =>
        {
            client.BaseAddress = new Uri(options.BaseAddress);
        });

        if (!string.IsNullOrEmpty(options.HealthCheckEndpoint))
        {
            services.AddHealthChecks().AddUrlGroup(
                new Uri(new Uri(options.BaseAddress), options.HealthCheckEndpoint),
                $"{typeof(TClient).Name}-health");
        }

        return services;
    }

    public static IServiceCollection AddGrpcServiceReference<TClient>(this IServiceCollection services, AspireOptions options)
        where TClient : class
    {
        if (!Uri.IsWellFormedUriString(options.BaseAddress, UriKind.Absolute))
            throw new ArgumentException("Base address must be a valid absolute URI.", nameof(options.BaseAddress));

        services.AddGrpcClient<TClient>(client =>
        {
            client.Address = new Uri(options.BaseAddress);
        });

        if (!string.IsNullOrEmpty(options.HealthCheckEndpoint))
        {
            services.AddGrpcHealthCheck<TClient>(new Uri(options.BaseAddress), $"{typeof(TClient).Name}-health");
        }

        return services;
    }

    public static IServiceCollection AddGrpcHealthCheck<TClient>(this IServiceCollection services, Uri baseAddress, string healthCheckName)
        where TClient : Grpc.Core.ClientBase<TClient>
    {
        services.AddHealthChecks().AddCheck(healthCheckName, new GrpcHealthCheck<TClient>(baseAddress));
        return services;
    }
}

public class GrpcHealthCheck<TClient> : IHealthCheck where TClient : Grpc.Core.ClientBase<TClient>
{
    private readonly Uri _serviceUri;

    public GrpcHealthCheck(Uri serviceUri)
    {
        _serviceUri = serviceUri;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        using var channel = GrpcChannel.ForAddress(_serviceUri);
        var client = Activator.CreateInstance(typeof(TClient), channel) as TClient;
        
        return HealthCheckResult.Healthy();
    }
}

public static class OpenTelemetryExtensions
{
    public static IServiceCollection ConfigureOpenTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        return services;
    }
}
