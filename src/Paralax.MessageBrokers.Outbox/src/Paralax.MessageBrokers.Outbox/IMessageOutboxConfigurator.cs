using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Paralax.MessageBrokers.Outbox
{
    public interface IMessageOutboxConfigurator
    {
        IParalaxBuilder Builder { get; }
        OutboxOptions Options { get; }
    }
}