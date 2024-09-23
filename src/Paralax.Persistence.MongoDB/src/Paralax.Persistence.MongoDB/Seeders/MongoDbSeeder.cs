using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Paralax.Persistence.MongoDB.Seeders
{
    internal class MongoDbSeeder : IMongoDbSeeder
    {
        /// <summary>
        /// Seeds the MongoDB database by invoking custom seed logic.
        /// </summary>
        /// <param name="database">The MongoDB database instance.</param>
        public async Task SeedAsync(IMongoDatabase database)
        {
            await CustomSeedAsync(database);
        }

        /// <summary>
        /// Custom seed logic that checks if the database has collections.
        /// If no collections exist, it will allow further seeding actions.
        /// </summary>
        /// <param name="database">The MongoDB database instance.</param>
        protected virtual async Task CustomSeedAsync(IMongoDatabase database)
        {
            var cursor = await database.ListCollectionsAsync();
            var collections = await cursor.ToListAsync();

            // If the database has existing collections, skip the seeding process.
            if (collections.Any())
            {
                return;
            }

            // You can add custom seeding logic here if no collections are found.
            await Task.CompletedTask;
        }
    }
}
