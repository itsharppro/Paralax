namespace Paralax.MessageBrokers.RabbitMQ
{
    public interface IExceptionToFailedMessageMapper
    {
        object Map(Exception exception, object message);
    }
}