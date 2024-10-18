namespace Paralax.MessageBrokers.RabbitMQ.Publishers
{
    internal sealed class RabbitMqPublisher : IBusPublisher
    {
        private readonly IEnumerable<IRabbitMqClient> _clients;
        private readonly IConventionsProvider _conventionsProvider;

        public RabbitMqPublisher(IEnumerable<IRabbitMqClient> clients, IConventionsProvider conventionsProvider)
        {
            _clients = clients;
            _conventionsProvider = conventionsProvider;
        }

        // Existing method: PublishAsync for the default or first broker
        public Task PublishAsync<T>(T message, string messageId = null, string correlationId = null,
            string spanContext = null, object messageContext = null, IDictionary<string, object> headers = null)
            where T : class
        {
            return PublishAsync(message, messageId, correlationId, spanContext, messageContext, headers, null);
        }

        // Modified method: PublishAsync with an optional brokerName parameter
        public Task PublishAsync<T>(T message, string messageId = null, string correlationId = null,
            string spanContext = null, object messageContext = null, IDictionary<string, object> headers = null, string brokerName = null)
            where T : class
        {
            // Get the client for the brokerName, or use the first client if no brokerName is provided or only one exists
            var client = GetClient(brokerName);
            var conventions = _conventionsProvider.Get(message.GetType());

            client.Send(message, conventions, messageId, correlationId, spanContext, messageContext, headers);

            return Task.CompletedTask;
        }

        public Task PublishToBrokerAsync<T>(T message, string brokerName, string messageId = null, string correlationId = null,
            string spanContext = null, object messageContext = null, IDictionary<string, object> headers = null)
            where T : class
        {
            var client = GetClient(brokerName); // Explicitly get the client based on the brokerName
            var conventions = _conventionsProvider.Get(message.GetType());

            client.Send(message, conventions, messageId, correlationId, spanContext, messageContext, headers);

            return Task.CompletedTask;
        }

        // Helper method to get the correct RabbitMQ client for a specific broker
        private IRabbitMqClient GetClient(string brokerName)
        {
            // Use the first client if brokerName is not provided or there is only one broker
            if (string.IsNullOrWhiteSpace(brokerName) || _clients.Count() == 1)
            {
                return _clients.First();
            }

            // Get the client for the provided brokerName
            var client = _clients.FirstOrDefault(c => (c as dynamic)?.ConnectionName == brokerName);
            if (client == null)
            {
                throw new ArgumentException($"No RabbitMQ client found for broker '{brokerName}'.");
            }

            return client;
        }
    }
}
