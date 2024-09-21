namespace Paralax.CQRS.EventSourcing
{
    public class EventStream
    {
        public Guid AggregateId { get; set; }
        public List<EventData> Events { get; set; } = new();
    }
}
