namespace Paralax.CQRS.EventSourcing
{
    public interface IEventStore
    {
        Task SaveEventsAsync(Guid aggregateId, IEnumerable<IEvent> events, int expectedVersion);
        Task<IEnumerable<IEvent>> GetEventsAsync(Guid aggregateId);
    }
}
