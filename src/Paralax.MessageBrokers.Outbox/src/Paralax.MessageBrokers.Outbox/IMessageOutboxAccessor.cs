using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Paralax.MessageBrokers.Outbox.Messages;

namespace Paralax.MessageBrokers.Outbox
{
    public interface IMessageOutboxAccessor
    {
        Task<IReadOnlyList<OutboxMessage>> GetUnsentAsync();
        Task ProcessAsync(OutboxMessage message);
        Task ProcessAsync(IEnumerable<OutboxMessage> outboxMessages);
    }
}