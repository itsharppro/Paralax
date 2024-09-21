using System;
using Paralax.CQRS.Events.Dispatchers;
using Microsoft.Extensions.DependencyInjection;

namespace Paralax.CQRS.Events
{
    public static class EventExtensions
    {
        /// <summary>
        /// Registers all event handlers implementing <see cref="IEventHandler{T}"/> in the current application domain assemblies.
        /// </summary>
        /// <param name="builder">The <see cref="IParalaxBuilder"/> instance.</param>
        /// <returns>The <see cref="IParalaxBuilder"/> instance.</returns>
        public static IParalaxBuilder AddEventHandlers(this IParalaxBuilder builder)
        {
            builder.Services.Scan(scan => scan
                .FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                .AddClasses(classes => classes.AssignableTo(typeof(IEventHandler<>)))
                .AsImplementedInterfaces()
                .WithTransientLifetime());

            return builder;
        }

        /// <summary>
        /// Registers the in-memory event dispatcher as a singleton.
        /// </summary>
        /// <param name="builder">The <see cref="IParalaxBuilder"/> instance.</param>
        /// <returns>The <see cref="IParalaxBuilder"/> instance.</returns>
        public static IParalaxBuilder AddInMemoryEventDispatcher(this IParalaxBuilder builder)
        {
            builder.Services.AddSingleton<IEventDispatcher, EventDispatcher>();
            return builder;
        }
    }
}
