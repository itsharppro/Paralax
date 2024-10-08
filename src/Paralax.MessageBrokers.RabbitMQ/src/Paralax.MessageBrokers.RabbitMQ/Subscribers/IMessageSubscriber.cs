using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Paralax.MessageBrokers.RabbitMQ.Subscribers
{
    internal interface IMessageSubscriber
    {
        MessageSubscriberAction Action { get; }
        Type Type { get; }
        Func<IServiceProvider, object, object, Task> Handle { get; }
    }
}