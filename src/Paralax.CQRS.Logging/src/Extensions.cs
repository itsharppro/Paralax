using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using Paralax.CQRS.Commands;
using Paralax.CQRS.Events;
using Paralax.CQRS.Logging.Decorators;

namespace Paralax.CQRS.Logging
{
    public static class Extensions
    {
        /// <summary>
        /// Adds logging decorators for all command handlers in the specified assembly.
        /// </summary>
        /// <param name="builder">The <see cref="IParalaxBuilder"/>.</param>
        /// <param name="assembly">The assembly to scan for command handlers.</param>
        /// <returns>The updated <see cref="IParalaxBuilder"/>.</returns>
        public static IParalaxBuilder AddCommandHandlersLogging(this IParalaxBuilder builder, Assembly assembly = null)
            => builder.AddHandlerLogging(typeof(ICommandHandler<>), typeof(CommandHandlerLoggingDecorator<>), assembly);

        /// <summary>
        /// Adds logging decorators for all event handlers in the specified assembly.
        /// </summary>
        /// <param name="builder">The <see cref="IParalaxBuilder"/>.</param>
        /// <param name="assembly">The assembly to scan for event handlers.</param>
        /// <returns>The updated <see cref="IParalaxBuilder"/>.</returns>
        public static IParalaxBuilder AddEventHandlersLogging(this IParalaxBuilder builder, Assembly assembly = null)
            => builder.AddHandlerLogging(typeof(IEventHandler<>), typeof(EventHandlerLoggingDecorator<>), assembly);

        /// <summary>
        /// Generic method to add logging decorators for either command or event handlers.
        /// </summary>
        private static IParalaxBuilder AddHandlerLogging(this IParalaxBuilder builder, Type handlerType,
            Type decoratorType, Assembly assembly = null)
        {
            assembly ??= Assembly.GetCallingAssembly();

            var handlers = assembly
                .GetTypes()
                .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerType))
                .ToList();

            handlers.ForEach(handler =>
            {
                // Find the TryDecorate method and invoke it on the appropriate service
                var extensionMethods = GetExtensionMethods();
                var tryDecorateMethod = extensionMethods.FirstOrDefault(mi => mi.Name == "TryDecorate" && !mi.IsGenericMethod);
                
                tryDecorateMethod?.Invoke(builder.Services, new object[]
                {
                    builder.Services,
                    handler.GetInterfaces().FirstOrDefault(),
                    decoratorType.MakeGenericType(handler.GetInterfaces().FirstOrDefault()?.GenericTypeArguments.First())
                });
            });

            return builder;
        }

        /// <summary>
        /// Retrieves the extension methods for service collection.
        /// </summary>
        private static IEnumerable<MethodInfo> GetExtensionMethods()
        {
            var types = typeof(ReplacementBehavior).Assembly.GetTypes();

            var query = from type in types
                        where type.IsSealed && !type.IsGenericType && !type.IsNested
                        from method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                        where method.IsDefined(typeof(ExtensionAttribute), false)
                        where method.GetParameters()[0].ParameterType == typeof(IServiceCollection)
                        select method;

            return query;
        }
    }
}
