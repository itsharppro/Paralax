using System;
using Microsoft.Extensions.DependencyInjection;
using Paralax.MessageBrokers.Outbox.Configurators;
using Paralax.MessageBrokers.Outbox.Outbox;
using Paralax.MessageBrokers.Outbox.Processors;

namespace Paralax.MessageBrokers.Outbox
{
    public static class Extensions
    {
        private const string SectionName = "outbox";
        private const string RegistryName = "messageBrokers.outbox";

        public static IParalaxBuilder AddMessageOutbox(this IParalaxBuilder builder,
            Action<IMessageOutboxConfigurator> configure = null, string sectionName = SectionName)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = SectionName;
            }

            if (!builder.TryRegister(RegistryName))
            {
                return builder;
            }

            var options = builder.GetOptions<OutboxOptions>(sectionName);
            builder.Services.AddSingleton(options);

            var configurator = new MessageOutboxConfigurator(builder, options);

            if (configure is null)
            {
                configurator.AddInMemory();
            }
            else
            {
                configure(configurator);
            }

            if (!options.Enabled)
            {
                return builder;
            }

            builder.Services.AddHostedService<OutboxProcessor>();

            return builder;
        }

        public static IMessageOutboxConfigurator AddInMemory(this IMessageOutboxConfigurator configurator)
        {
            configurator.Builder.Services.AddTransient<IMessageOutbox, InMemoryMessageOutbox>();

            return configurator;
        }
    }
}
