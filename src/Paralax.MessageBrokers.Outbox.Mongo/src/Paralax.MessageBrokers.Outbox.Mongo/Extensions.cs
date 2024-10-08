
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using Paralax.MessageBrokers.Outbox.Messages;
using Paralax.MessageBrokers.Outbox.Mongo.Core;
using Paralax.Persistence.MongoDB;

namespace Paralax.MessageBrokers.Outbox.Mongo
{
    public static class Extensions
    {
        public static IMessageOutboxConfigurator AddMongo(this IMessageOutboxConfigurator configurator)
        {
            var builder = configurator.Builder;
            var options = configurator.Options;

            var inboxCollection = string.IsNullOrWhiteSpace(options.InboxCollection)
                ? "inbox"
                : options.InboxCollection;
            var outboxCollection = string.IsNullOrWhiteSpace(options.OutboxCollection)
                ? "outbox"
                : options.OutboxCollection;

            builder.AddMongoRepository<InboxMessage, string>(inboxCollection);
            builder.AddMongoRepository<OutboxMessage, string>(outboxCollection);
            builder.AddInitializer<MongoOutboxInitializer>();
            builder.Services.AddTransient<IMessageOutbox, MongoMessageOutbox>();
            builder.Services.AddTransient<IMessageOutboxAccessor, MongoMessageOutbox>();
            builder.Services.AddTransient<MongoOutboxInitializer>();

            BsonClassMap.RegisterClassMap<OutboxMessage>(m =>
            {
                m.AutoMap();
                m.UnmapMember(p => p.Message);
                m.UnmapMember(p => p.MessageContext);
            });

            return configurator;
        }
    }
}