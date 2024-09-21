namespace Paralax.CQRS.EventSourcing
{
    public class EventStoreRepository<T> : IEventStoreRepository<T> where T : IEventSourced, new()
    {
        private readonly IEventStore _eventStore;

        public EventStoreRepository(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public async Task SaveAsync(T aggregate, int expectedVersion)
        {
            var uncommittedEvents = aggregate.GetUncommittedEvents();
            await _eventStore.SaveEventsAsync(aggregate.Id, uncommittedEvents, expectedVersion);
            aggregate.MarkEventsAsCommitted();
        }

        public async Task<T> GetByIdAsync(Guid aggregateId)
        {
            var events = await _eventStore.GetEventsAsync(aggregateId);
            var aggregate = new T();
            aggregate.LoadFromHistory(events);
            return aggregate;
        }
    }
}
