using System;
using Microsoft.Extensions.DependencyInjection;
using Paralax.CQRS.Queries;
using Paralax.CQRS.Queries.Dispatchers;
using Paralax.Types;

namespace Paralax.Extensions
{
    public static class QueryHandlerExtensions
    {
        /// <summary>
        /// Scans and registers all query handlers implementing IQueryHandler<TQuery, TResult> in the application assemblies.
        /// </summary>
        /// <param name="builder">The IParalaxBuilder instance.</param>
        /// <returns>The IParalaxBuilder instance.</returns>
        public static IParalaxBuilder AddQueryHandlers(this IParalaxBuilder builder)
        {
            builder.Services.Scan(scan => scan
                .FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)))
                .AsImplementedInterfaces()
                .WithTransientLifetime());

            return builder;
        }

        /// <summary>
        /// Registers the in-memory query dispatcher as a singleton.
        /// </summary>
        /// <param name="builder">The IParalaxBuilder instance.</param>
        /// <returns>The IParalaxBuilder instance.</returns>
        public static IParalaxBuilder AddInMemoryQueryDispatcher(this IParalaxBuilder builder)
        {
            builder.Services.AddSingleton<IQueryDispatcher, QueryDispatcher>();
            return builder;
        }
    }
}
