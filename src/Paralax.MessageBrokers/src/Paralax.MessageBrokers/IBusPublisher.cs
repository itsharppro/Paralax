using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Paralax.MessageBrokers
{
    public interface IBusPublisher
    {
        Task PublishAsync<T>(T message, string messageId = null, string correlationId = null, string spanContext = null,
        object messageContext = null, IDictionary<string, object> headers = null) where T : class;
    }
}