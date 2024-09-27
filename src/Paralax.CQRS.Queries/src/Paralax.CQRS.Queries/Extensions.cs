using System;
using Microsoft.Extensions.DependencyInjection;
using Paralax.Core;
using Paralax.CQRS.Queries; 
using Paralax.CQRS.Queries.Dispatchers;

namespace Paralax.CQRS.Queries
{
    public static class Extensions
    {
        public static IParalaxBuilder AddQueryHandlers(this IParalaxBuilder builder)
        {
            builder.Services.Scan(scan =>
                scan.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                    .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>))
                        .WithoutAttribute(typeof(DecoratorAttribute)))
                    .AsImplementedInterfaces()
                    .WithTransientLifetime());

            return builder;
        }

        public static IParalaxBuilder AddInMemoryQueryDispatcher(this IParalaxBuilder builder)
        {
            builder.Services.AddSingleton<IQueryDispatcher, QueryDispatcher>();
            return builder;
        }
    }
}
