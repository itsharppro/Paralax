using MongoDB.Driver;

namespace Paralax.Persistence.MongoDB
{
    public interface IMongoDbSeeder
    {
        Task SeedAsync(IMongoDatabase database);
    }
}