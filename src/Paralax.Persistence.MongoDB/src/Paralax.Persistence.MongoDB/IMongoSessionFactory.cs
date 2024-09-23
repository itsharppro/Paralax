using MongoDB.Driver;

namespace Paralax.Persistence.MongoDB
{
    public interface IMongoSessionFactory
    {
        Task<IClientSessionHandle> CreateAsync();
    }
}