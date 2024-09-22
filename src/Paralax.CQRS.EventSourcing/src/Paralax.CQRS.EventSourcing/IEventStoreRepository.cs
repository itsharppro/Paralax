namespace Paralax.CQRS.EventSourcing
{
    public interface IEventStoreRepository<T> where T : IEventSourced
    {
        Task SaveAsync(T aggregate, int expectedVersion);
        Task<T> GetByIdAsync(Guid aggregateId);
    }
}
