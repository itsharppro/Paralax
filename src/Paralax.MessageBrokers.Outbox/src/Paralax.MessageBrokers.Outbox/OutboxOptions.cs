namespace Paralax.MessageBrokers.Outbox
{
    public class OutboxOptions
    {
        // Determines whether the outbox feature is enabled
        public bool Enabled { get; set; } = true;

        // Specifies how long messages stay in the outbox before expiry (in minutes)
        public int Expiry { get; set; } = 1440; // default 1 day

        // The interval at which outbox messages are processed (in milliseconds)
        public double IntervalMilliseconds { get; set; } = 5000; // default 5 seconds

        // The name of the collection/table for inbox (for deduplication)
        public string InboxCollection { get; set; }

        // The name of the collection/table for outbox (for storing outgoing messages)
        public string OutboxCollection { get; set; }

        // Defines the type of outbox (e.g., SQL, MongoDB, etc.)
        public string Type { get; set; } = "SQL"; 

        // Option to disable transaction-based processing
        public bool DisableTransactions { get; set; } = false;

        // Enables retries for failed message sends
        public bool EnableRetries { get; set; } = true;

        // The maximum number of retry attempts for failed messages
        public int MaxRetryAttempts { get; set; } = 3;

        // The delay (in milliseconds) between retry attempts
        public int RetryDelayMilliseconds { get; set; } = 2000;

        // Specifies whether to log outbox activities (e.g., sending, retries, failures)
        public bool EnableOutboxLogging { get; set; } = true;

        // Option to clean up expired or processed outbox entries
        public bool AutoCleanup { get; set; } = true;

        // The interval for running auto-cleanup tasks (in minutes)
        public int CleanupIntervalMinutes { get; set; } = 1440; // default 1 day

        // Specifies whether to bypass the outbox and send messages directly when the system is under heavy load
        public bool BypassOnHighLoad { get; set; } = false;

        // Threshold (in terms of pending messages) to trigger the bypass mechanism
        public int HighLoadThreshold { get; set; } = 100;

        // Timeout for sending a message via the outbox (in milliseconds)
        public int SendTimeoutMilliseconds { get; set; } = 10000; // default 10 seconds
    }
}
