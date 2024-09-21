namespace Paralax.CQRS.EventSourcing
{
    public interface IEventSourced : IAggregateRoot
    {
        void LoadFromHistory(IEnumerable<IEvent> history);
    }
}
