using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Paralax.MessageBrokers.RabbitMQ
{
    public interface IRabbitMqPluginsRegistry
    {
        IRabbitMqPluginsRegistry Add<TPlugin>() 
            where TPlugin : class, IRabbitMqPlugin;
    }
}