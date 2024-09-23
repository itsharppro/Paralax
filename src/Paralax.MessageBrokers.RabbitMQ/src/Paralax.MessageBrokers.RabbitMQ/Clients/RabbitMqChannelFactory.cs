using RabbitMQ.Client;

namespace Paralax.MessageBrokers.RabbitMQ.Clients
{
    public class RabbitMqChannelFactory : IRabbitMqChannelFactory
    {
        private readonly ProducerConnection _producerConnection;
        private readonly RabbitMqOptions _options;

        public RabbitMqChannelFactory(ProducerConnection producerConnection, RabbitMqOptions options)
        {
            _producerConnection = producerConnection;
            _options = options;
        }

        public IModel CreateChannel()
        {
            var channel = _producerConnection.Connection.CreateModel();

            // Configure QoS (Quality of Service) settings
            channel.BasicQos(_options.Qos.PrefetchSize, _options.Qos.PrefetchCount, _options.Qos.Global);

            return channel;
        }
    }
}
