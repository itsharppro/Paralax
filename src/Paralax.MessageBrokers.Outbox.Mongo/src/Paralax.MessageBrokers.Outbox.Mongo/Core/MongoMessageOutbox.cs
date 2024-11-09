using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Paralax.MessageBrokers.Outbox.Messages;
using Paralax.Persistence.MongoDB;
using MongoDB.Driver;
using NetJSON;

namespace Paralax.MessageBrokers.Outbox.Mongo.Core
{
    internal sealed class MongoMessageOutbox : IMessageOutbox, IMessageOutboxAccessor
    {
        private readonly IMongoSessionFactory _sessionFactory;
        private readonly IMongoRepository<InboxMessage, string> _inboxRepository;
        private readonly IMongoRepository<OutboxMessage, string> _outboxRepository;
        private readonly ILogger<MongoMessageOutbox> _logger;
        private readonly bool _transactionsEnabled;

        public bool Enabled { get; }

        public MongoMessageOutbox(IMongoSessionFactory sessionFactory,
            IMongoRepository<InboxMessage, string> inboxRepository,
            IMongoRepository<OutboxMessage, string> outboxRepository,
            OutboxOptions options, ILogger<MongoMessageOutbox> logger)
        {
            _sessionFactory = sessionFactory;
            _inboxRepository = inboxRepository;
            _outboxRepository = outboxRepository;
            _logger = logger;
            _transactionsEnabled = !options.DisableTransactions;
            Enabled = options.Enabled;
        }

        public async Task HandleAsync(string messageId, Func<Task> handler)
        {
            if (!Enabled)
            {
                _logger.LogWarning("Outbox is disabled, incoming messages won't be processed.");
                return;
            }

            if (string.IsNullOrWhiteSpace(messageId))
            {
                throw new ArgumentException("Message ID to be processed cannot be empty.", nameof(messageId));
            }

            _logger.LogTrace($"Received a message with ID: '{messageId}' to be processed.");
            if (await _inboxRepository.ExistsAsync(m => m.Id == messageId))
            {
                _logger.LogTrace($"Message with ID: '{messageId}' was already processed.");
                return;
            }

            IClientSessionHandle session = null;
            if (_transactionsEnabled)
            {
                session = await _sessionFactory.CreateAsync();
                session.StartTransaction();
            }

            try
            {
                _logger.LogTrace($"Processing a message with ID: '{messageId}'...");
                await handler();
                await _inboxRepository.AddAsync(new InboxMessage
                {
                    Id = messageId,
                    ProcessedAt = DateTime.UtcNow
                });

                if (session != null)
                {
                    await session.CommitTransactionAsync();
                }

                _logger.LogTrace($"Processed a message with ID: '{messageId}'.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing message with ID: '{messageId}'.");
                if (session != null)
                {
                    await session.AbortTransactionAsync();
                }

                throw;
            }
            finally
            {
                session?.Dispose();
            }
        }

        public async Task SendAsync<T>(T message, string originatedMessageId = null, string messageId = null,
            string correlationId = null, string spanContext = null, object messageContext = null,
            IDictionary<string, object> headers = null) where T : class
        {
            if (!Enabled)
            {
                _logger.LogWarning("Outbox is disabled, outgoing messages won't be saved into storage.");
                return;
            }

            // Serialize message and context to JSON and then encode to byte array
            var serializedMessageContext = messageContext == null
                ? Array.Empty<byte>()
                : Encoding.UTF8.GetBytes(NetJSON.NetJSON.Serialize(messageContext));

            var serializedMessage = message == null
                ? Array.Empty<byte>()
                : Encoding.UTF8.GetBytes(NetJSON.NetJSON.Serialize(message));

            var outboxMessage = new OutboxMessage
            {
                Id = string.IsNullOrWhiteSpace(messageId) ? Guid.NewGuid().ToString("N") : messageId,
                OriginatedMessageId = originatedMessageId,
                CorrelationId = correlationId,
                SpanContext = spanContext,
                SerializedMessageContext = serializedMessageContext,
                MessageContextType = messageContext?.GetType().AssemblyQualifiedName,
                Headers = headers != null ? new Dictionary<string, object>(headers) : null,
                SerializedMessage = serializedMessage,
                MessageType = message?.GetType().AssemblyQualifiedName,
                SentAt = DateTime.UtcNow
            };

            await _outboxRepository.AddAsync(outboxMessage);
        }

        async Task<IReadOnlyList<OutboxMessage>> IMessageOutboxAccessor.GetUnsentAsync()
        {
            var outboxMessages = await _outboxRepository.FindAsync(om => om.ProcessedAt == null);
            return outboxMessages.Select(om =>
            {
                if (!string.IsNullOrWhiteSpace(om.MessageContextType))
                {
                    var messageContextType = Type.GetType(om.MessageContextType);
                    if (messageContextType != null && om.SerializedMessageContext.Length > 0)
                    {
                        var contextJson = Encoding.UTF8.GetString(om.SerializedMessageContext);
                        om.MessageContext = NetJSON.NetJSON.Deserialize(messageContextType, contextJson);
                    }
                }

                if (!string.IsNullOrWhiteSpace(om.MessageType))
                {
                    var messageType = Type.GetType(om.MessageType);
                    if (messageType != null && om.SerializedMessage.Length > 0)
                    {
                        var messageJson = Encoding.UTF8.GetString(om.SerializedMessage);
                        om.Message = NetJSON.NetJSON.Deserialize(messageType, messageJson);
                    }
                }

                return om;
            }).ToList();
        }

        Task IMessageOutboxAccessor.ProcessAsync(OutboxMessage message)
            => _outboxRepository.Collection.UpdateOneAsync(
                Builders<OutboxMessage>.Filter.Eq(m => m.Id, message.Id),
                Builders<OutboxMessage>.Update.Set(m => m.ProcessedAt, DateTime.UtcNow));

        Task IMessageOutboxAccessor.ProcessAsync(IEnumerable<OutboxMessage> outboxMessages)
            => _outboxRepository.Collection.UpdateManyAsync(
                Builders<OutboxMessage>.Filter.In(m => m.Id, outboxMessages.Select(m => m.Id)),
                Builders<OutboxMessage>.Update.Set(m => m.ProcessedAt, DateTime.UtcNow));
    }
}
