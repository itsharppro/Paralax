using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Paralax.Docs.Scalar;
using System;

namespace Paralax.WebApi.Scalar
{
    public static class ScalarExtensions
    {
        private const string DefaultRoutePrefix = "scalar";

        /// <summary>
        /// Adds Scalar API documentation to the Paralax Web API.
        /// </summary>
        /// <param name="builder">Paralax builder for web APIs.</param>
        /// <param name="configureOptions">Optional: Custom configuration for Scalar API documentation.</param>
        public static IParalaxBuilder AddScalarDocs(this IParalaxBuilder builder, Action<ScalarOptions>? configureOptions = null)
        {
            // Configure the Scalar options and add OpenAPI services
            builder.Services.AddOpenApi(); // Adds OpenAPI support, required for Scalar.

            // Configure options for Scalar docs if provided
            if (configureOptions != null)
            {
                builder.Services.Configure(configureOptions);
            }

            return builder;
        }

        /// <summary>
        /// Configures the application to use Scalar for API documentation.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="configureOptions">Optional: Custom configuration for Scalar API documentation UI.</param>
        public static IApplicationBuilder UseScalarDocs(this IApplicationBuilder app, Action<ScalarOptions>? configureOptions = null)
        {
            // Get Scalar options or apply custom configuration
            var options = app.ApplicationServices.GetService<ScalarOptions>() ?? new ScalarOptions();

            if (configureOptions != null)
            {
                configureOptions.Invoke(options);
            }

            return app;
        }
    }
}
