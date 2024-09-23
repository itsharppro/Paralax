namespace Paralax.MessageBrokers
{
    public interface IMessageProperties
    {
        string MessageId { get; }
        string CorrelationId { get; }
        long Timestamp { get; }
        IDictionary<string, object> Headers { get; }
    }
}