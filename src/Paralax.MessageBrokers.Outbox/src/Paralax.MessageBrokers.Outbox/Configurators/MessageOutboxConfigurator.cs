using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Paralax.MessageBrokers.Outbox.Configurators
{
    internal sealed class MessageOutboxConfigurator : IMessageOutboxConfigurator
    {
        public IParalaxBuilder Builder { get; }
        public OutboxOptions Options { get; }

        public MessageOutboxConfigurator(IParalaxBuilder builder, OutboxOptions options)
        {
            Builder = builder;
            Options = options;
        }
    }
}