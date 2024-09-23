using System;
using System.Collections.Generic;

namespace Paralax.MessageBrokers.RabbitMQ
{
    public class RabbitMqOptions
    {
        // Connection and Authentication
        public string ConnectionName { get; set; }
        public IEnumerable<string> HostNames { get; set; }
        public int Port { get; set; } = 5672;
        public string VirtualHost { get; set; } = "/";
        public string Username { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        
        // Timeouts and Network Settings
        public TimeSpan RequestedHeartbeat { get; set; } = TimeSpan.FromSeconds(60);
        public TimeSpan RequestedConnectionTimeout { get; set; } = TimeSpan.FromSeconds(30);
        public TimeSpan SocketReadTimeout { get; set; } = TimeSpan.FromSeconds(30);
        public TimeSpan SocketWriteTimeout { get; set; } = TimeSpan.FromSeconds(30);
        public TimeSpan ContinuationTimeout { get; set; } = TimeSpan.FromSeconds(20);
        public TimeSpan HandshakeContinuationTimeout { get; set; } = TimeSpan.FromSeconds(10);
        public TimeSpan NetworkRecoveryInterval { get; set; } = TimeSpan.FromSeconds(5);
        public TimeSpan? MessageProcessingTimeout { get; set; }
        
        // Channel and Frame Settings
        public ushort RequestedChannelMax { get; set; } = 0;
        public uint RequestedFrameMax { get; set; } = 0;
        public bool UseBackgroundThreadsForIO { get; set; } = false;
        public string ConventionsCasing { get; set; }
        public int Retries { get; set; } = 3;
        public int RetryInterval { get; set; } = 1000;
        public bool MessagesPersisted { get; set; } = false;
        public int MaxProducerChannels { get; set; } = 1000;
        public bool RequeueFailedMessages { get; set; } = false;

        // Logging Options
        public LoggerOptions Logger { get; set; } = new LoggerOptions();
        
        // Context Options
        public ContextOptions Context { get; set; } = new ContextOptions();
        
        // Exchange Options
        public ExchangeOptions Exchange { get; set; } = new ExchangeOptions();
        
        // Queue Options
        public QueueOptions Queue { get; set; } = new QueueOptions();
        
        // Dead Letter Queue Options
        public DeadLetterOptions DeadLetter { get; set; } = new DeadLetterOptions();
        
        // Quality of Service (QoS) Options
        public QosOptions Qos { get; set; } = new QosOptions();
        
        // Conventions for Exchange, Routing Key, etc.
        public ConventionsOptions Conventions { get; set; } = new ConventionsOptions();
        
        // Span Context for Tracing
        public string SpanContextHeader { get; set; } = "span_context";

        // Get Span Context Header Method
        public string GetSpanContextHeader()
            => string.IsNullOrWhiteSpace(SpanContextHeader) ? "span_context" : SpanContextHeader;

        // Nested Classes for Options

        public class LoggerOptions
        {
            public bool Enabled { get; set; } = false;
            public bool LogConnectionStatus { get; set; } = false;
            public bool LogMessagePayload { get; set; } = false;
        }

        public class ContextOptions
        {
            public bool Enabled { get; set; } = true;
            public string Header { get; set; } = "message_context";
        }

        public class ExchangeOptions
        {
            public string Name { get; set; } = "default";
            public string Type { get; set; } = "direct";
            public bool Declare { get; set; } = true;
            public bool Durable { get; set; } = true;
            public bool AutoDelete { get; set; } = false;
        }

        public class QueueOptions
        {
            public string Template { get; set; }
            public bool Declare { get; set; } = true;
            public bool Durable { get; set; } = true;
            public bool Exclusive { get; set; } = false;
            public bool AutoDelete { get; set; } = false;
        }

        public class DeadLetterOptions
        {
            public bool Enabled { get; set; } = false;
            public string Prefix { get; set; }
            public string Suffix { get; set; }
            public bool Declare { get; set; } = true;
            public bool Durable { get; set; } = true;
            public bool Exclusive { get; set; } = false;
            public bool AutoDelete { get; set; } = false;
            public int? Ttl { get; set; }
        }

        public class SslOptions
        {
            public bool Enabled { get; set; } = false;
            public string ServerName { get; set; }
            public string CertificatePath { get; set; }
            public string CaCertificatePath { get; set; }
            public IEnumerable<string> X509IgnoredStatuses { get; set; }
        }

        public class QosOptions
        {
            public uint PrefetchSize { get; set; } = 0;
            public ushort PrefetchCount { get; set; } = 10;
            public bool Global { get; set; } = false;
        }

        public class ConventionsOptions
        {
            public MessageAttributeOptions MessageAttribute { get; set; } = new MessageAttributeOptions();
            
            public class MessageAttributeOptions
            {
                public bool IgnoreExchange { get; set; } = false;
                public bool IgnoreRoutingKey { get; set; } = false;
                public bool IgnoreQueue { get; set; } = false;
            }
        }
    }
}
