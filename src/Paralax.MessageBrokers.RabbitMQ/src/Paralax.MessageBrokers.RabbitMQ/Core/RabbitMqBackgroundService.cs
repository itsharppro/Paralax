using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Paralax.MessageBrokers.RabbitMQ.Plugins;
using Paralax.MessageBrokers.RabbitMQ.Subscribers;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Paralax.MessageBrokers.RabbitMQ.Internals
{
    internal sealed class RabbitMqBackgroundService : BackgroundService
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            WriteIndented = true
        };

        private readonly ConcurrentDictionary<string, IModel> _channels = new();
        private readonly IServiceProvider _serviceProvider;
        private readonly IConnection _consumerConnection;
        private readonly IConnection _producerConnection;
        private readonly MessageSubscribersChannel _messageSubscribersChannel;
        private readonly IBusPublisher _publisher;
        private readonly IRabbitMqSerializer _rabbitMqSerializer;
        private readonly IConventionsProvider _conventionsProvider;
        private readonly IContextProvider _contextProvider;
        private readonly ILogger _logger;
        private readonly IRabbitMqPluginsExecutor _pluginsExecutor;
        private readonly IExceptionToMessageMapper _exceptionToMessageMapper;
        private readonly IExceptionToFailedMessageMapper _exceptionToFailedMessageMapper;
        private readonly int _retries;
        private readonly int _retryInterval;
        private readonly bool _loggerEnabled;
        private readonly bool _logMessagePayload;
        private readonly RabbitMqOptions _options;
        private readonly RabbitMqOptions.QosOptions _qosOptions;
        private readonly bool _requeueFailedMessages;

        public RabbitMqBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _consumerConnection = serviceProvider.GetRequiredService<ConsumerConnection>().Connection;
            _producerConnection = serviceProvider.GetRequiredService<ProducerConnection>().Connection;
            _messageSubscribersChannel = serviceProvider.GetRequiredService<MessageSubscribersChannel>();
            _publisher = serviceProvider.GetRequiredService<IBusPublisher>();
            _rabbitMqSerializer = serviceProvider.GetRequiredService<IRabbitMqSerializer>();
            _conventionsProvider = serviceProvider.GetRequiredService<IConventionsProvider>();
            _contextProvider = serviceProvider.GetRequiredService<IContextProvider>();
            _logger = serviceProvider.GetRequiredService<ILogger<RabbitMqSubscriber>>();
            _pluginsExecutor = serviceProvider.GetRequiredService<IRabbitMqPluginsExecutor>();
            _options = serviceProvider.GetRequiredService<RabbitMqOptions>();
            _exceptionToMessageMapper = serviceProvider.GetService<IExceptionToMessageMapper>() ?? new EmptyExceptionToMessageMapper();
            _exceptionToFailedMessageMapper = serviceProvider.GetService<IExceptionToFailedMessageMapper>() ?? new EmptyExceptionToFailedMessageMapper();
            _loggerEnabled = _options.Logger?.Enabled ?? false;
            _logMessagePayload = _options.Logger?.LogMessagePayload ?? false;
            _retries = _options.Retries >= 0 ? _options.Retries : 3;
            _retryInterval = _options.RetryInterval > 0 ? _options.RetryInterval : 2;
            _qosOptions = _options.Qos ?? new RabbitMqOptions.QosOptions();
            _requeueFailedMessages = _options.RequeueFailedMessages;
            if (_qosOptions.PrefetchCount < 1)
            {
                _qosOptions.PrefetchCount = 1;
            }

            RegisterConnectionEvents();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var messageSubscriber in _messageSubscribersChannel.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    switch (messageSubscriber.Action)
                    {
                        case MessageSubscriberAction.Subscribe:
                            Subscribe(messageSubscriber);
                            break;
                        case MessageSubscriberAction.Unsubscribe:
                            Unsubscribe(messageSubscriber);
                            break;
                        default:
                            throw new InvalidOperationException("Unknown message subscriber action type.");
                    }
                }
                catch (Exception exception)
                {
                    _logger.LogError($"Error during RabbitMQ action: '{messageSubscriber.Action}'.");
                    _logger.LogError(exception, exception.Message);
                }
            }
        }

        private void Subscribe(IMessageSubscriber messageSubscriber)
        {
            var conventions = _conventionsProvider.Get(messageSubscriber.Type);
            var channelKey = GetChannelKey(conventions);

            if (_channels.ContainsKey(channelKey))
            {
                return;
            }

            var channel = _consumerConnection.CreateModel();
            if (!_channels.TryAdd(channelKey, channel))
            {
                _logger.LogError($"Couldn't add the channel for {channelKey}.");
                channel.Dispose();
                return;
            }

            DeclareQueue(channel, conventions);
            SetupConsumer(channel, messageSubscriber, conventions);
        }

        private void DeclareQueue(IModel channel, IConventions conventions)
        {
            var declare = _options.Queue?.Declare ?? true;
            var durable = _options.Queue?.Durable ?? true;
            var exclusive = _options.Queue?.Exclusive ?? false;
            var autoDelete = _options.Queue?.AutoDelete ?? false;

            if (declare)
            {
                _logger.LogInformation($"Declaring a queue: '{conventions.Queue}' for exchange: '{conventions.Exchange}'.");

                var queueArguments = new Dictionary<string, object>();
                if (_options.DeadLetter?.Enabled is true)
                {
                    queueArguments["x-dead-letter-exchange"] = $"{_options.DeadLetter.Prefix}{_options.Exchange.Name}{_options.DeadLetter.Suffix}";
                }

                channel.QueueDeclare(conventions.Queue, durable, exclusive, autoDelete, queueArguments);
            }

            channel.QueueBind(conventions.Queue, conventions.Exchange, conventions.RoutingKey);
            channel.BasicQos(_qosOptions.PrefetchSize, _qosOptions.PrefetchCount, _qosOptions.Global);
        }

        private void SetupConsumer(IModel channel, IMessageSubscriber messageSubscriber, IConventions conventions)
        {
            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.Received += async (_, args) =>
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();

                    var messageId = args.BasicProperties.MessageId;
                    var correlationId = args.BasicProperties.CorrelationId;
                    var message = _rabbitMqSerializer.Deserialize(args.Body.Span, messageSubscriber.Type);

                    if (_loggerEnabled)
                    {
                        var messagePayload = _logMessagePayload ? Encoding.UTF8.GetString(args.Body.Span) : string.Empty;
                        _logger.LogInformation("Received message with ID: '{MessageId}', Correlation ID: '{CorrelationId}', Payload: '{MessagePayload}'",
                            messageId, correlationId, messagePayload);
                    }

                    var correlationContext = BuildCorrelationContext(scope, args);
                    await HandleMessageAsync(channel, message, messageId, correlationId, correlationContext, args, messageSubscriber.Handle);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                    channel.BasicNack(args.DeliveryTag, false, _requeueFailedMessages);
                }
            };

            channel.BasicConsume(conventions.Queue, false, consumer);
        }

        private async Task HandleMessageAsync(IModel channel, object message, string messageId, string correlationId,
            object messageContext, BasicDeliverEventArgs args, Func<IServiceProvider, object, object, Task> handle)
        {
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(_retries, _ => TimeSpan.FromSeconds(_retryInterval));

            await retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    await handle(_serviceProvider, message, messageContext);
                    channel.BasicAck(args.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                    throw;
                }
            });
        }

        private object BuildCorrelationContext(IServiceScope scope, BasicDeliverEventArgs args)
        {
            var messagePropertiesAccessor = scope.ServiceProvider.GetRequiredService<IMessagePropertiesAccessor>();
            messagePropertiesAccessor.MessageProperties = new MessageProperties
            {
                MessageId = args.BasicProperties.MessageId,
                CorrelationId = args.BasicProperties.CorrelationId,
                Timestamp = args.BasicProperties.Timestamp.UnixTime,
                Headers = args.BasicProperties.Headers
            };

            var correlationContextAccessor = scope.ServiceProvider.GetRequiredService<ICorrelationContextAccessor>();
            var correlationContext = _contextProvider.Get(args.BasicProperties.Headers);
            correlationContextAccessor.CorrelationContext = correlationContext;

            return correlationContext;
        }

        private void Unsubscribe(IMessageSubscriber messageSubscriber)
        {
            var conventions = _conventionsProvider.Get(messageSubscriber.Type);
            var channelKey = GetChannelKey(conventions);

            if (!_channels.TryRemove(channelKey, out var channel))
            {
                return;
            }

            channel.Dispose();
        }

        private static string GetChannelKey(IConventions conventions)
            => $"{conventions.Exchange}:{conventions.Queue}:{conventions.RoutingKey}";

        private void RegisterConnectionEvents()
        {
            if (!_loggerEnabled || _options.Logger?.LogConnectionStatus != true)
            {
                return;
            }

            _consumerConnection.CallbackException += ConnectionOnCallbackException;
            _consumerConnection.ConnectionShutdown += ConnectionOnConnectionShutdown;
            _consumerConnection.ConnectionBlocked += ConnectionOnConnectionBlocked;
            _consumerConnection.ConnectionUnblocked += ConnectionOnConnectionUnblocked;

            _producerConnection.CallbackException += ConnectionOnCallbackException;
            _producerConnection.ConnectionShutdown += ConnectionOnConnectionShutdown;
            _producerConnection.ConnectionBlocked += ConnectionOnConnectionBlocked;
            _producerConnection.ConnectionUnblocked += ConnectionOnConnectionUnblocked;
        }

        private void ConnectionOnCallbackException(object sender, CallbackExceptionEventArgs eventArgs)
        {
            _logger.LogError("RabbitMQ callback exception occurred.", eventArgs.Exception);
        }

        private void ConnectionOnConnectionShutdown(object sender, ShutdownEventArgs eventArgs)
        {
            _logger.LogError($"RabbitMQ connection shutdown occurred. Reply code: {eventArgs.ReplyCode}, Text: {eventArgs.ReplyText}");
        }

        private void ConnectionOnConnectionBlocked(object sender, ConnectionBlockedEventArgs eventArgs)
        {
            _logger.LogError($"RabbitMQ connection blocked. Reason: {eventArgs.Reason}");
        }

        private void ConnectionOnConnectionUnblocked(object sender, EventArgs eventArgs)
        {
            _logger.LogInformation("RabbitMQ connection unblocked.");
        }

        public override void Dispose()
        {
            foreach (var (_, channel) in _channels)
            {
                channel?.Dispose();
            }

            _consumerConnection.Close();
            _producerConnection.Close();

            base.Dispose();
        }

        private class EmptyExceptionToMessageMapper : IExceptionToMessageMapper
        {
            public object Map(Exception exception, object message) => null;
        }

        private class EmptyExceptionToFailedMessageMapper : IExceptionToFailedMessageMapper
        {
            public FailedMessage Map(Exception exception, object message) => null;
        }
    }
}
