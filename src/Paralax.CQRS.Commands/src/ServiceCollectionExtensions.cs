using System;
using Microsoft.Extensions.DependencyInjection;
using Paralax;
using Paralax.Types;
using Paralax.CQRS.Commands;
using Paralax.CQRS.Commands.Dispatchers;

namespace Paralax.CQRS.Commands
{
    public static class ParalaxBuilderExtensions
    {
        /// <summary>
        /// Adds command handlers implementing ICommandHandler<T> in the current application domain assemblies.
        /// </summary>
        /// <param name="builder">The IParalaxBuilder instance.</param>
        /// <returns>The IParalaxBuilder instance.</returns>
        public static IParalaxBuilder AddCommandHandlers(this IParalaxBuilder builder)
        {
            builder.Services.Scan(scan => scan
                .FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)))
                .AsImplementedInterfaces()
                .WithTransientLifetime());

            return builder;
        }

        /// <summary>
        /// Registers the in-memory command dispatcher as a singleton.
        /// </summary>
        /// <param name="builder">The IParalaxBuilder instance.</param>
        /// <returns>The IParalaxBuilder instance.</returns>
        public static IParalaxBuilder AddInMemoryCommandDispatcher(this IParalaxBuilder builder)
        {
            builder.Services.AddSingleton<ICommandDispatcher, CommandDispatcher>();
            return builder;
        }
    }
}
