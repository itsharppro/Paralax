using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Paralax.Types;

namespace Paralax.MessageBrokers.Outbox.Messages
{
    public sealed class InboxMessage : IIdentifiable<string>
    {
        public string Id { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
}