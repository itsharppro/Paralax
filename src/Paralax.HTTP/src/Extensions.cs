using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;

namespace Paralax.HTTP
{
    public static class Extensions
    {
        private const string SectionName = "httpClient";
        private const string RegistryName = "http.client";

        public static IParalaxBuilder AddHttpClient(this IParalaxBuilder builder, string clientName = "paralax",
            IEnumerable<string> maskedRequestUrlParts = null, string sectionName = SectionName,
            Action<IHttpClientBuilder> httpClientBuilder = null)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = SectionName;
            }

            if (!builder.TryRegister(RegistryName))
            {
                return builder;
            }

            if (string.IsNullOrWhiteSpace(clientName))
            {
                throw new ArgumentException("HTTP client name cannot be empty.", nameof(clientName));
            }

            // Fetch HttpClientOptions from configuration
            var options = builder.GetOptions<HttpClientOptions>(sectionName);
            if (maskedRequestUrlParts is not null && options.RequestMasking is not null)
            {
                options.RequestMasking.UrlParts = maskedRequestUrlParts;
            }

            // Determine if correlation factories need to be registered
            bool registerCorrelationContextFactory;
            bool registerCorrelationIdFactory;

            using (var scope = builder.Services.BuildServiceProvider().CreateScope())
            {
                registerCorrelationContextFactory = scope.ServiceProvider.GetService<ICorrelationContextFactory>() is null;
                registerCorrelationIdFactory = scope.ServiceProvider.GetService<ICorrelationIdFactory>() is null;
            }

            // Register correlation context and ID factories if necessary
            if (registerCorrelationContextFactory)
            {
                builder.Services.AddSingleton<ICorrelationContextFactory, EmptyCorrelationContextFactory>();
            }

            if (registerCorrelationIdFactory)
            {
                builder.Services.AddSingleton<ICorrelationIdFactory, EmptyCorrelationIdFactory>();
            }

            // Register options and serializers
            builder.Services.AddSingleton(options);
            builder.Services.AddSingleton<IHttpClientSerializer, SystemTextJsonHttpClientSerializer>();

            // Register the HTTP client with logging and any additional customizations
            var clientBuilder = builder.Services.AddHttpClient<IHttpClient, ParalaxHttpClient>(clientName);
            httpClientBuilder?.Invoke(clientBuilder);

            if (options.RequestMasking?.Enabled == true)
            {
                // Replace the default HttpMessageHandlerBuilderFilter with the custom ParalaxLoggingHttpMessageScopeHandler
                builder.Services.Replace(ServiceDescriptor
                    .Singleton<IHttpMessageHandlerBuilderFilter, ParalaxHttpLoggingFilter>());
            }

            return builder;
        }

        [Description("This is a hack related to HttpClient issue: https://github.com/aspnet/AspNetCore/issues/13346")]
        public static void RemoveHttpClient(this IParalaxBuilder builder)
        {
            var registryType = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
                .SingleOrDefault(t => t.Name == "HttpClientMappingRegistry");
            var registry = builder.Services.SingleOrDefault(s => s.ServiceType == registryType)?.ImplementationInstance;
            var registrations = registry?.GetType().GetProperty("TypedClientRegistrations");
            var clientRegistrations = registrations?.GetValue(registry) as IDictionary<Type, string>;
            clientRegistrations?.Remove(typeof(IHttpClient));
        }
    }
}
