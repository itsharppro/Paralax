using RabbitMQ.Client;

namespace Paralax.MessageBrokers.RabbitMQ
{
    public interface IRabbitMqChannelFactory
    {
        IModel CreateChannel();
    }
}