using RabbitMQ.Client.Events;

namespace Paralax.MessageBrokers.RabbitMQ
{
    public interface IRabbitMqPlugin
    {
        Task HandleAsync(object message, object correlationContext, 
            BasicDeliverEventArgs args);
    }
}