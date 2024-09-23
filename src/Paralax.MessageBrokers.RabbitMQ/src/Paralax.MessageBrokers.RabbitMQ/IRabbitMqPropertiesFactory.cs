using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace Paralax.MessageBrokers.RabbitMQ
{
    public interface IRabbitMqPropertiesFactory
    {
        IBasicProperties CreateProperties(IModel channel, object message, string messageId, string correlationId, 
            string spanContext, object messageContext, IDictionary<string, object> headers);
    }
}