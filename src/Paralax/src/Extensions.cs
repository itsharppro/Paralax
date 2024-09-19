using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Paralax.Core;
using Paralax.Types;
using Figletize; 

namespace Paralax
{
    public static class Extensions
    {
        private const string SectionName = "app";

        public static IParalaxBuilder AddParalax(this IServiceCollection services, string sectionName = SectionName, IConfiguration configuration = null)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = SectionName;
            }

            var builder = ParalaxBuilder.Create(services, configuration);
            var options = builder.GetOptions<AppOptions>(sectionName);

            builder.Services.AddMemoryCache();
            builder.Services.AddSingleton(options);
            builder.Services.AddSingleton<IServiceId, ServiceId>();

            if (options.DisplayBanner && !string.IsNullOrWhiteSpace(options.Name))
            {
                var version = options.DisplayVersion ? $" {options.Version}" : string.Empty;
                var fullMessage = $"{options.Name}{version}";

                string figletText = FigletTools.RenderFiglet(fullMessage, "banner");

                var fontInstance = FigletizeFonts.TryGetByName("banner");
                if (fontInstance == null)
                {
                    Console.Error.WriteLine("Font 'banner' not found. Using default.");
                }
                else
                {
                    figletText = fontInstance.Render(fullMessage);
                }

                Console.WriteLine(figletText);
            }

            return builder;
        }

        public static IApplicationBuilder UseParalax(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var initializer = scope.ServiceProvider.GetRequiredService<IStartupInitializer>();

            Task.Run(() => initializer.InitializeAsync()).GetAwaiter().GetResult();

            return app;
        }

        public static TModel GetOptions<TModel>(this IConfiguration configuration, string sectionName)
            where TModel : new()
        {
            var model = new TModel();
            configuration.GetSection(sectionName).Bind(model);
            return model;
        }

        public static TModel GetOptions<TModel>(this IParalaxBuilder builder, string sectionName)
            where TModel : new()
        {
            if (builder.Configuration != null)
            {
                return builder.Configuration.GetOptions<TModel>(sectionName);
            }

            using var serviceProvider = builder.Services.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            return configuration.GetOptions<TModel>(sectionName);
        }

        public static string Underscore(this string value)
            => string.Concat(value.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x : x.ToString())).ToLowerInvariant();
    }
}
