using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Paralax.MessageBrokers.Outbox
{
    public interface IMessageOutbox
    {
        bool Enabled { get; }

        Task HandleAsync(string messageId, Func<Task> handler);

        Task SendAsync<T>(T message, string originatedMessageId = null, string messageId = null,
            string correlationId = null, string spanContext = null, object messageContext = null,
            IDictionary<string, object> headers = null) where T : class;
    }
}
