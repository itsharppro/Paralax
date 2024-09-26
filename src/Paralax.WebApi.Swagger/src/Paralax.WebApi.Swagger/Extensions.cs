using System;
using Microsoft.Extensions.DependencyInjection;
using Paralax.Docs.Swagger;
using Paralax.WebApi.Swagger.Filters;

namespace Paralax.WebApi.Swagger
{
    public static class Extensions
    {
        private const string SectionName = "swagger";

        public static IParalaxBuilder AddWebApiSwaggerDocs(this IParalaxBuilder builder, string sectionName = SectionName)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = SectionName;
            }

            return builder.AddWebApiSwaggerDocs(b => b.AddSwaggerDocs(sectionName));
        }

        public static IParalaxBuilder AddWebApiSwaggerDocs(this IParalaxBuilder builder, 
            Func<ISwaggerOptionsBuilder, ISwaggerOptionsBuilder> buildOptions)
            => builder.AddWebApiSwaggerDocs(b => b.AddSwaggerDocs(buildOptions));
        
        public static IParalaxBuilder AddWebApiSwaggerDocs(this IParalaxBuilder builder, SwaggerOptions options)
            => builder.AddWebApiSwaggerDocs(b => b.AddSwaggerDocs(options));

        private static IParalaxBuilder AddWebApiSwaggerDocs(this IParalaxBuilder builder, Action<IParalaxBuilder> registerSwagger)
        {
            registerSwagger(builder);
            builder.Services.AddSwaggerGen(c => c.DocumentFilter<WebApiDocumentFilter>());
            return builder;
        }
    }
}
