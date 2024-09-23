using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Paralax.MessageBrokers.RabbitMQ
{
    public sealed class ConsumerConnection
    {
        public IConnection Connection { get; }

        public ConsumerConnection(IConnection connection)
        {
            Connection = connection;
        }
    }
}