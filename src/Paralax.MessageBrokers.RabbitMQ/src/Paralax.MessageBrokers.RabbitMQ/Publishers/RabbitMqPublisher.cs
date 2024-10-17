using System.Collections.Generic;
using System.Threading.Tasks;

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

        public Task PublishAsync<T>(T message, string messageId = null, string correlationId = null,
            string spanContext = null, object messageContext = null, IDictionary<string, object> headers = null)
            where T : class
        {
            return PublishAsync(message, messageId, correlationId, spanContext, messageContext, headers, null);
        }

        public Task PublishAsync<T>(T message, string messageId = null, string correlationId = null,
            string spanContext = null, object messageContext = null, IDictionary<string, object> headers = null, string brokerName = null)
            where T : class
        {
            var client = GetClient(brokerName);
            var conventions = _conventionsProvider.Get(message.GetType());

            client.Send(message, conventions, messageId, correlationId, spanContext, messageContext, headers);

            return Task.CompletedTask;
        }

        private IRabbitMqClient GetClient(string brokerName)
        {
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
    }
}
