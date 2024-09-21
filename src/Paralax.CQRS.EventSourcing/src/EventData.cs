namespace Paralax.CQRS.EventSourcing
{
    public class EventData
    {
        public Guid AggregateId { get; set; }
        public string EventType { get; set; }
        public string Data { get; set; }
        public int Version { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
