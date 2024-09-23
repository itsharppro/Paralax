using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Paralax.Persistence.MongoDB.Initializers
{
    internal sealed class MongoDbInitializer : IMongoDbInitializer
    {
        private static int _initialized;
        private readonly bool _seed;
        private readonly IMongoDatabase _database;
        private readonly IMongoDbSeeder _seeder;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbInitializer"/> class.
        /// </summary>
        /// <param name="database">MongoDB database instance.</param>
        /// <param name="seeder">Seeder used to populate the database with initial data if needed.</param>
        /// <param name="options">MongoDB options that define if seeding should be performed.</param>
        public MongoDbInitializer(IMongoDatabase database, IMongoDbSeeder seeder, MongoDbOptions options)
        {
            _database = database;
            _seeder = seeder;
            _seed = options.Seed;
        }

        /// <summary>
        /// Initializes the MongoDB database by checking if seeding is necessary.
        /// </summary>
        public Task InitializeAsync()
        {
            // Ensure initialization happens only once
            if (Interlocked.Exchange(ref _initialized, 1) == 1)
            {
                return Task.CompletedTask;
            }

            // Seed the database if required
            return _seed ? _seeder.SeedAsync(_database) : Task.CompletedTask;
        }
    }
}
