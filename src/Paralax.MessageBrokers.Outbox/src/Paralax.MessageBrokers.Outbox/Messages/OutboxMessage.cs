using System;
using System.Collections.Generic;
using Paralax.Types;

namespace Paralax.MessageBrokers.Outbox.Messages
{
    public sealed class OutboxMessage : IIdentifiable<string>
    {
        // Unique identifier for the message
        public string Id { get; set; }
        
        // ID of the original message that triggered this outbox message
        public string OriginatedMessageId { get; set; }
        
        // Correlation ID for tracking the message flow across multiple systems
        public string CorrelationId { get; set; }
        
        // Span context for distributed tracing
        public string SpanContext { get; set; }
        
        // Headers for any metadata related to the message
        public Dictionary<string, object> Headers { get; set; } = new();

        // Type of the message being stored in the outbox
        public string MessageType { get; set; }

        // Type of the message context (optional) for any additional context passed along with the message
        public string MessageContextType { get; set; }
        
        // The message object that contains the actual payload to be sent
        public object Message { get; set; }

        // Additional context related to the message (optional)
        public object MessageContext { get; set; }

        // Serialized version of the message for persistence or transport
        public string SerializedMessage { get; set; }

        // Serialized version of the message context for persistence or transport
        public string SerializedMessageContext { get; set; }

        // The time when the message was added to the outbox and is ready to be sent
        public DateTime SentAt { get; set; }

        // The time when the message was successfully processed and sent
        public DateTime? ProcessedAt { get; set; }
    }
}
