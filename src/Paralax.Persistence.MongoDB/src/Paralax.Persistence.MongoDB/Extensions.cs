using System;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Paralax.Persistence.MongoDB.Builders;
using Paralax.Persistence.MongoDB.Factories;
using Paralax.Persistence.MongoDB.Initializers;
using Paralax.Persistence.MongoDB.Repositories;
using Paralax.Persistence.MongoDB.Seeders;
using Paralax.Types;

namespace Paralax.Persistence.MongoDB
{
    public static class Extensions
    {
        private static bool _conventionsRegistered;
        private const string SectionName = "mongo";
        private const string RegistryName = "persistence.mongoDb";

        public static IParalaxBuilder AddMongo(this IParalaxBuilder builder, string sectionName = SectionName, Type seederType = null, bool registerConventions = true)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = SectionName;
            }

            var mongoOptions = builder.GetOptions<MongoDbOptions>(sectionName);
            return builder.AddMongo(mongoOptions, seederType, registerConventions);
        }

        public static IParalaxBuilder AddMongo(this IParalaxBuilder builder, Func<IMongoDbOptionsBuilder, IMongoDbOptionsBuilder> buildOptions, Type seederType = null, bool registerConventions = true)
        {
            var mongoOptions = buildOptions(new MongoDbOptionsBuilder()).Build();
            return builder.AddMongo(mongoOptions, seederType, registerConventions);
        }

        public static IParalaxBuilder AddMongo(this IParalaxBuilder builder, MongoDbOptions mongoOptions, Type seederType = null, bool registerConventions = true)
        {
            if (!builder.TryRegister(RegistryName))
            {
                return builder;
            }

            if (mongoOptions.SetRandomDatabaseSuffix)
            {
                var suffix = $"{Guid.NewGuid():N}";
                Console.WriteLine($"Setting a random MongoDB database suffix: '{suffix}'.");
                mongoOptions.Database = $"{mongoOptions.Database}_{suffix}";
            }

            builder.Services.AddSingleton(mongoOptions);
            builder.Services.AddSingleton<IMongoClient>(sp =>
            {
                var options = sp.GetRequiredService<MongoDbOptions>();
                return new MongoClient(options.ConnectionString);
            });
            builder.Services.AddTransient(sp =>
            {
                var options = sp.GetRequiredService<MongoDbOptions>();
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(options.Database);
            });
            builder.Services.AddTransient<IMongoDbInitializer, MongoDbInitializer>();
            builder.Services.AddTransient<IMongoSessionFactory, MongoSessionFactory>();

            if (seederType is null)
            {
                builder.Services.AddTransient<IMongoDbSeeder, MongoDbSeeder>();
            }
            else
            {
                builder.Services.AddTransient(typeof(IMongoDbSeeder), seederType);
            }

            builder.AddInitializer<IMongoDbInitializer>();

            if (registerConventions && !_conventionsRegistered)
            {
                RegisterConventions();
            }

            return builder;
        }

        private static void RegisterConventions()
        {
            _conventionsRegistered = true;
            BsonSerializer.RegisterSerializer(typeof(decimal), new DecimalSerializer(BsonType.Decimal128));
            BsonSerializer.RegisterSerializer(typeof(decimal?), new NullableSerializer<decimal>(new DecimalSerializer(BsonType.Decimal128)));
            ConventionRegistry.Register("paralax", new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new IgnoreExtraElementsConvention(true),
                new EnumRepresentationConvention(BsonType.String),
            }, _ => true);
        }

        // Updated this method to register the IMongoDbCollection instead of using IMongoCollection directly
        public static IParalaxBuilder AddMongoRepository<TEntity, TIdentifiable>(this IParalaxBuilder builder, string collectionName)
            where TEntity : class, IIdentifiable<TIdentifiable>
        {
            builder.Services.AddTransient<IMongoRepository<TEntity, TIdentifiable>>(sp =>
            {
                var database = sp.GetRequiredService<IMongoDatabase>();
                var collection = new MongoDbCollection<TEntity>(database, collectionName); // Register MongoDbCollection here
                return new MongoRepository<TEntity, TIdentifiable>(collection);
            });

            return builder;
        }
    }
}
