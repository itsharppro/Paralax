using RabbitMQ.Client;

namespace Paralax.MessageBrokers.RabbitMQ
{
    public sealed class ProducerConnection
    {
        public IConnection Connection { get; }

        public ProducerConnection(IConnection connection)
        {
            Connection = connection;
        }
    }
}