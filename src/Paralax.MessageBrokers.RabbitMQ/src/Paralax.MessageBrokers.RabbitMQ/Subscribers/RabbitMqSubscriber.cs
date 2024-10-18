namespace Paralax.MessageBrokers.RabbitMQ.Subscribers
{
    internal sealed class RabbitMqSubscriber : IBusSubscriber, IDisposable
    {
        private readonly IEnumerable<IRabbitMqClient> _clients; 
        private readonly MessageSubscribersChannel _messageSubscribersChannel;

        public RabbitMqSubscriber(IEnumerable<IRabbitMqClient> clients, MessageSubscribersChannel messageSubscribersChannel)
        {
            _clients = clients;
            _messageSubscribersChannel = messageSubscribersChannel;
        }

        // Subscribe method: Subscribes to the default or first broker
        public IBusSubscriber Subscribe<T>(Func<IServiceProvider, T, object, Task> handle) where T : class
        {
            foreach (var client in _clients)
            {
                InternalSubscribe(client, handle);
            }
            return this;
        }

        // Modified method: Subscribe to a specific broker or default if brokerName is null
        public IBusSubscriber SubscribeToBroker<T>(Func<IServiceProvider, T, object, Task> handle, string brokerName) where T : class
        {
            var client = GetClient(brokerName);
            return InternalSubscribe(client, handle);
        }

        private IBusSubscriber InternalSubscribe<T>(IRabbitMqClient client, Func<IServiceProvider, T, object, Task> handle) where T : class
        {
            var type = typeof(T);

            _messageSubscribersChannel.Writer.TryWrite(
                MessageSubscriber.Subscribe(type, (serviceProvider, message, context) => handle(serviceProvider, (T)message, context))
            );

            return this;
        }

        // Helper method to get the correct RabbitMQ client for a specific broker
        private IRabbitMqClient GetClient(string brokerName)
        {
            // Use the first client if brokerName is not provided or there is only one broker
            if (string.IsNullOrWhiteSpace(brokerName) || _clients.Count() == 1)
            {
                return _clients.First();
            }

            var client = _clients.FirstOrDefault(c => (c as dynamic)?.ConnectionName == brokerName);
            if (client == null)
            {
                throw new ArgumentException($"No RabbitMQ client found for broker '{brokerName}'.");
            }

            return client;
        }

        public void Dispose()
        {
            // Cleanup logic if necessary
        }
    }
}
