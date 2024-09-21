using Paralax.CQRS.Events;

namespace Paralax.CQRS.EventSourcing
{
    public interface IAggregateRoot
    {
        Guid Id { get; }
        int Version { get; }
        IEnumerable<IEvent> GetUncommittedEvents();
        void MarkEventsAsCommitted();
        void ApplyEvent(IEvent @event);
    }
}
