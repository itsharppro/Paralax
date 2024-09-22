using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Paralax.Docs.Scalar.Builders;
using Scalar.AspNetCore;
using System;

namespace Paralax.Docs.Scalar
{
    public static class Extensions
    {
        private const string SectionName = "scalar";
        private const string RegistryName = "docs.scalar";

        public static IParalaxBuilder AddScalarDocs(this IParalaxBuilder builder, string sectionName = SectionName)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = SectionName;
            }

            var options = builder.GetOptions<ScalarOptions>(sectionName);
            return builder.AddScalarDocs(options);
        }

        public static IParalaxBuilder AddScalarDocs(this IParalaxBuilder builder,
            Func<IScalarOptionsBuilder, IScalarOptionsBuilder> buildOptions)
        {
            var options = buildOptions(new ScalarOptionsBuilder()).Build();
            return builder.AddScalarDocs(options);
        }

        public static IParalaxBuilder AddScalarDocs(this IParalaxBuilder builder, ScalarOptions options)
        {
            if (!options.Enabled || !builder.TryRegister(RegistryName))
            {
                return builder;
            }

            builder.Services.AddOpenApi();  

            // if (options.IncludeSecurity)
            // {
            //     builder.Services.AddOpenApi(opt =>
            //     {
            //         opt.UseTransformer<BearerSecuritySchemeTransformer>();
            //     });
            // }

            builder.Services.AddSingleton(options);
            return builder;
        }

        public static IApplicationBuilder UseScalarDocs(this IApplicationBuilder app)
        {
            var options = app.ApplicationServices.GetRequiredService<ScalarOptions>();
            if (!options.Enabled)
            {
                return app;
            }

            var routePrefix = string.IsNullOrWhiteSpace(options.RoutePrefix) ? string.Empty : options.RoutePrefix;

            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapScalarApiReference();
            });

            return app;
        }
    }
}
