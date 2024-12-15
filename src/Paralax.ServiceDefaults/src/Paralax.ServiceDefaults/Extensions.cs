// using Paralax.Net.Http;
// using Paralax.gRPC;
using Paralax.Diagnostics.HealthChecks;
using Paralax.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Diagnostics.HealthChecks;
using Grpc.Net.Client;
using Microsoft.Extensions.Hosting;
using Grpc.Health.V1;



namespace Paralax.ServiceDefaults;

using AddHealthChecks = Paralax.Diagnostics.HealthChecks;

public static class ServiceReferenceExtensions
{

    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();

            // Turn on service discovery by default
            http.AddServiceDiscovery();
        });

        return builder;
    }

    public static IParalaxBuilder AddServiceDefaults(this IParalaxBuilder builder)
    {
        // builder.ConfigureOpenTelemetry();
        // builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });


        return builder;
    }




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
             Microsoft.Extensions.DependencyInjection.HealthCheckServiceCollectionExtensions
                .AddHealthChecks(services)
                .AddUrlGroup(
                new Uri(new Uri(options.BaseAddress), options.HealthCheckEndpoint),
                $"{typeof(TClient).Name}-health");
        }

        return services;
    }

   public static IHttpClientBuilder AddGrpcServiceReference<TClient>(this IServiceCollection services, string address)
        where TClient : class
    {
        ArgumentNullException.ThrowIfNull(services);

        if (!Uri.IsWellFormedUriString(address, UriKind.Absolute))
        {
            throw new ArgumentException("Address must be a valid absolute URI.", nameof(address));
        }

        var builder = services.AddGrpcClient<TClient>(o => o.Address = new(address));

        return builder;
    }


     public static IHttpClientBuilder AddGrpcServiceReference<TClient>(this IServiceCollection services, string address, string? healthCheckName = null, HealthStatus failureStatus = default)
        where TClient : class
    {
        ArgumentNullException.ThrowIfNull(services);

        if (!Uri.IsWellFormedUriString(address, UriKind.Absolute))
        {
            throw new ArgumentException("Address must be a valid absolute URI.", nameof(address));
        }

        var uri = new Uri(address);
        var builder = services.AddGrpcClient<TClient>(o => o.Address = uri);

        AddGrpcHealthChecks(services, uri, healthCheckName ?? $"{typeof(TClient).Name}-health", failureStatus);

        return builder;
    }

    private static void AddGrpcHealthChecks(IServiceCollection services, Uri uri, string healthCheckName, HealthStatus failureStatus = default)
    {
        services.AddGrpcClient<Health.HealthClient>(o => o.Address = uri);
        Paralax.Diagnostics.HealthChecks.HealthChecksServiceCollectionExtensions
            .AddHealthChecks(services)
            .AddCheck<GrpcServiceHealthCheck>(healthCheckName, failureStatus);
    }

    private sealed class GrpcServiceHealthCheck(Health.HealthClient healthClient) : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var response = await healthClient.CheckAsync(new(), cancellationToken: cancellationToken);

            return response.Status switch
            {
                HealthCheckResponse.Types.ServingStatus.Serving => HealthCheckResult.Healthy(),
                _ => HealthCheckResult.Unhealthy()
            };
        }
    }
}