namespace Paralax.MessageBrokers.RabbitMQ
{
    public interface IExceptionToMessageMapper
    {
        FailedMessage Map(Exception exception, object message);
    }
}