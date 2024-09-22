using System.Collections.Concurrent;
using Paralax.CQRS.Events;

namespace Paralax.CQRS.EventSourcing
{
    public class InMemoryEventStore : IEventStore
    {
        private readonly ConcurrentDictionary<Guid, List<IEvent>> _store = new();

        public Task SaveEventsAsync(Guid aggregateId, IEnumerable<IEvent> events, int expectedVersion)
        {
            var eventList = _store.GetOrAdd(aggregateId, _ => new List<IEvent>());
            eventList.AddRange(events);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<IEvent>> GetEventsAsync(Guid aggregateId)
        {
            _store.TryGetValue(aggregateId, out var events);
            return Task.FromResult(events ?? Enumerable.Empty<IEvent>());
        }
    }
}
