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

        // Implement the SubscribeToBroker method as declared in the interface
        public IBusSubscriber SubscribeToBroker<T>(Func<IServiceProvider, T, object, Task> handle, string brokerName) where T : class
        {
            // Get the RabbitMQ client for the specified brokerName
            var client = GetClient(brokerName);

            // Subscribe to the specific broker
            return InternalSubscribe(client, handle);
        }

        // Internal method to perform the subscription
        private IBusSubscriber InternalSubscribe<T>(IRabbitMqClient client, Func<IServiceProvider, T, object, Task> handle) where T : class
        {
            var type = typeof(T);

            // Subscribe the message handler to the message subscriber channel
            _messageSubscribersChannel.Writer.TryWrite(
                MessageSubscriber.Subscribe(type, (serviceProvider, message, context) => handle(serviceProvider, (T)message, context))
            );

            return this;
        }

        // Helper method to get the correct RabbitMQ client for the given broker name
        private IRabbitMqClient GetClient(string brokerName)
        {
            if (string.IsNullOrWhiteSpace(brokerName) || _clients.Count() == 1)
            {
                return _clients.First();
            }

            // Find the client with the matching brokerName
            var client = _clients.FirstOrDefault(c => (c as dynamic)?.ConnectionName == brokerName);
            if (client == null)
            {
                throw new ArgumentException($"No RabbitMQ client found for broker '{brokerName}'.");
            }

            return client;
        }

        public void Dispose()
        {
            // Cleanup resources if needed
        }
    }
}
