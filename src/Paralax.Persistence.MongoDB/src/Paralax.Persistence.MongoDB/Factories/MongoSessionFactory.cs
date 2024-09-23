using System.Threading.Tasks;
using MongoDB.Driver;

namespace Paralax.Persistence.MongoDB.Factories
{
    internal sealed class MongoSessionFactory : IMongoSessionFactory
    {
        private readonly IMongoClient _client;

        public MongoSessionFactory(IMongoClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Creates a new MongoDB client session asynchronously.
        /// </summary>
        /// <returns>An active MongoDB client session handle.</returns>
        public Task<IClientSessionHandle> CreateAsync()
        {
            return _client.StartSessionAsync();
        }
    }
}
