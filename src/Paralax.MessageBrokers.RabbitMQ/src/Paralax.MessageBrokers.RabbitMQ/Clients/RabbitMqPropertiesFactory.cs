using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace Paralax.MessageBrokers.RabbitMQ.Clients
{
    public class RabbitMqPropertiesFactory : IRabbitMqPropertiesFactory
    {
        private readonly IContextProvider _contextProvider;
        private readonly IRabbitMqSerializer _serializer;
        private readonly RabbitMqOptions _options;

        public RabbitMqPropertiesFactory(IContextProvider contextProvider, IRabbitMqSerializer serializer, RabbitMqOptions options)
        {
            _contextProvider = contextProvider ?? throw new ArgumentNullException(nameof(contextProvider));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public IBasicProperties CreateProperties(IModel channel, object message, string messageId, string correlationId, 
            string spanContext, object messageContext, IDictionary<string, object> headers)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));

            var properties = channel.CreateBasicProperties();
            properties.Persistent = _options.MessagesPersisted;
            properties.MessageId = string.IsNullOrWhiteSpace(messageId) ? Guid.NewGuid().ToString("N") : messageId;
            properties.CorrelationId = string.IsNullOrWhiteSpace(correlationId) ? Guid.NewGuid().ToString("N") : correlationId;
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.Headers = new Dictionary<string, object>();

            // Include message context if enabled
            if (_options.Context?.Enabled == true)
            {
                IncludeMessageContext(properties, messageContext);
            }

            // Add span context header
            if (!string.IsNullOrWhiteSpace(spanContext))
            {
                properties.Headers[_options.GetSpanContextHeader()] = spanContext;
            }

            // Add custom headers
            if (headers != null)
            {
                foreach (var (key, value) in headers)
                {
                    if (!string.IsNullOrWhiteSpace(key) && value != null)
                    {
                        properties.Headers.TryAdd(key, value);
                    }
                }
            }

            return properties;
        }

        private void IncludeMessageContext(IBasicProperties properties, object context)
        {
            if (properties == null) throw new ArgumentNullException(nameof(properties));

            if (context != null)
            {
                properties.Headers[_contextProvider.HeaderName] = _serializer.Serialize(context).ToArray();
            }
            else
            {
                properties.Headers[_contextProvider.HeaderName] = "{}";
            }
        }
    }
}
