using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Paralax.MessageBrokers.RabbitMQ.Plugins
{
    internal sealed class RabbitMqPluginChain
    {
        public Type PluginType { get; set; }
    }
}