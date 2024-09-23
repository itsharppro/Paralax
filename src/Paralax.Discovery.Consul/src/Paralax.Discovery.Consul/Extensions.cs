using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Paralax.Discovery.Consul.Builders;
using Paralax.Discovery.Consul.Http;
using Paralax.Discovery.Consul.MessageHandlers;
using Paralax.Discovery.Consul.Models;
using Paralax.Discovery.Consul.Services;
using Paralax.HTTP;
using Paralax.Types;

namespace Paralax.Discovery.Consul;

public static class Extensions
{
    private const string DefaultInterval = "5s";
    private const string SectionName = "consul";
    private const string RegistryName = "discovery.consul";

    public static IParalaxBuilder AddConsul(this IParalaxBuilder builder, string sectionName = SectionName, string httpClientSectionName = "httpClient")
    {
        sectionName ??= SectionName;
        var consulOptions = builder.GetOptions<ConsulOptions>(sectionName);
        var httpClientOptions = builder.GetOptions<HttpClientOptions>(httpClientSectionName);

        return builder.AddConsul(consulOptions, httpClientOptions);
    }

    public static IParalaxBuilder AddConsul(this IParalaxBuilder builder, Func<IConsulOptionsBuilder, IConsulOptionsBuilder> buildOptions, HttpClientOptions httpClientOptions)
    {
        var options = buildOptions(new ConsulOptionsBuilder()).Build();
        return builder.AddConsul(options, httpClientOptions);
    }

    public static IParalaxBuilder AddConsul(this IParalaxBuilder builder, ConsulOptions options, HttpClientOptions httpClientOptions)
    {
        builder.Services.AddSingleton(options);

        if (!options.Enabled || !builder.TryRegister(RegistryName))
        {
            return builder;
        }

        if (httpClientOptions.Type?.Equals("consul", StringComparison.OrdinalIgnoreCase) == true)
        {
            builder.Services.AddTransient<ConsulServiceDiscoveryMessageHandler>();
            builder.Services.AddHttpClient<IConsulHttpClient, ConsulHttpClient>("consul-http")
                .AddHttpMessageHandler<ConsulServiceDiscoveryMessageHandler>();

            // Add the Consul HttpClient
            builder.Services.AddHttpClient<IHttpClient, ConsulHttpClient>("consul")
                .AddHttpMessageHandler<ConsulServiceDiscoveryMessageHandler>();
        }

        // Register Consul service registry
        builder.Services.AddTransient<IConsulServicesRegistry, ConsulServicesRegistry>();

        var registration = builder.CreateConsulAgentRegistration(options);
        if (registration != null)
        {
            builder.Services.AddSingleton(registration);
        }

        return builder;
    }

    public static void AddConsulHttpClient(this IParalaxBuilder builder, string clientName, string serviceName)
    {
        builder.Services.AddHttpClient<IHttpClient, ConsulHttpClient>(clientName)
            .AddHttpMessageHandler(c => new ConsulServiceDiscoveryMessageHandler(
                c.GetRequiredService<IConsulServicesRegistry>(),
                c.GetRequiredService<ConsulOptions>(),
                serviceName, 
                true));
    }

    // Helper method to create Consul agent registration
    private static ServiceRegistration CreateConsulAgentRegistration(this IParalaxBuilder builder, ConsulOptions options)
    {
        var enabled = options.Enabled;
        var consulEnabled = Environment.GetEnvironmentVariable("CONSUL_ENABLED")?.ToLowerInvariant();

        if (!string.IsNullOrWhiteSpace(consulEnabled))
        {
            enabled = consulEnabled is "true" or "1";
        }

        if (!enabled)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(options.Address))
        {
            throw new ArgumentException("Consul address cannot be empty.", nameof(options.Address));
        }

        // Register the Consul service client
        builder.Services.AddHttpClient<IConsulService, ConsulService>(c => c.BaseAddress = new Uri(options.Url));

        // Ensure that the hosted service is only added once
        if (!builder.Services.Any(x => x.ServiceType == typeof(ConsulHostedService)))
        {
            builder.Services.AddHostedService<ConsulHostedService>();
        }

        // Use the service provider to resolve IServiceId
        string serviceId;
        using (var serviceProvider = builder.Services.BuildServiceProvider())
        {
            serviceId = serviceProvider.GetRequiredService<IServiceId>().Id;
        }

        var registration = new ServiceRegistration
        {
            Name = options.Service,
            Id = $"{options.Service}:{serviceId}",
            Address = options.Address,
            Port = options.Port,
            Tags = options.Tags,
            Meta = options.Meta,
            EnableTagOverride = options.EnableTagOverride,
            Connect = options.Connect?.Enabled == true ? new Connect() : null
        };

        if (!options.PingEnabled)
        {
            return registration;
        }

        // Build health check details
        var pingEndpoint = string.IsNullOrWhiteSpace(options.PingEndpoint) ? string.Empty :
            options.PingEndpoint.StartsWith("/") ? options.PingEndpoint : $"/{options.PingEndpoint}";

        var scheme = options.Address.StartsWith("http", StringComparison.InvariantCultureIgnoreCase) ? string.Empty : "http://";
        var check = new ServiceCheck
        {
            Interval = ParseTime(options.PingInterval),
            DeregisterCriticalServiceAfter = ParseTime(options.RemoveAfterInterval),
            Http = $"{scheme}{options.Address}:{options.Port}{pingEndpoint}"
        };

        registration.Checks = new[] { check };

        return registration;
    }

    // Parse the time format for interval or timeout
    private static string ParseTime(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? DefaultInterval : int.TryParse(value, out var number) ? $"{number}s" : value;
    }
}
