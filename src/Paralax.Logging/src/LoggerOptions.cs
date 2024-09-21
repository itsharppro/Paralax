using Paralax.Logging.Options;

namespace Paralax.Logging 
{
    public class LoggerOptions
    {
        /// <summary>
        /// The logging level (e.g., Information, Debug, Warning, Error).
        /// </summary>
        public string Level { get; set; } = "Information";

        /// <summary>
        /// Options for logging to the console.
        /// </summary>
        public ConsoleOptions Console { get; set; } = new ConsoleOptions();

        /// <summary>
        /// Options for logging to a file.
        /// </summary>
        public FileLoggingOptions File { get; set; } = new FileLoggingOptions();

        /// <summary>
        /// Options for logging to an ELK (Elasticsearch, Logstash, Kibana) stack.
        /// </summary>
        public ElkOptions Elk { get; set; } = new ElkOptions();

        /// <summary>
        /// Options for logging to Seq.
        /// </summary>
        public SeqOptions Seq { get; set; } = new SeqOptions();

        /// <summary>
        /// Options for logging to Grafana Loki.
        /// </summary>
        public LokiOptions Loki { get; set; } = new LokiOptions();

        /// <summary>
        /// Overrides for minimum log levels for specific namespaces or classes.
        /// </summary>
        public IDictionary<string, string> MinimumLevelOverrides { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Paths to exclude from logging (e.g., health checks, static files).
        /// </summary>
        public IEnumerable<string> ExcludePaths { get; set; } = new List<string>();

        /// <summary>
        /// Properties to exclude from logging (e.g., sensitive information like passwords).
        /// </summary>
        public IEnumerable<string> ExcludeProperties { get; set; } = new List<string>();

        /// <summary>
        /// Custom tags to apply to all log entries.
        /// </summary>
        public IDictionary<string, object> Tags { get; set; } = new Dictionary<string, object>();
    }
}