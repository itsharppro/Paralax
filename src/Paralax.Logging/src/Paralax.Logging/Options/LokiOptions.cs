namespace Paralax.Logging.Options
{
    public class LokiOptions
    {
        /// <summary>
        /// Enable or disable Loki logging.
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// The URI of the Grafana Loki server.
        /// </summary>
        public string Url { get; set; } = "http://localhost:3100";

        /// <summary>
        /// Specify the log level for Loki output.
        /// </summary>
        public string Level { get; set; } = "Information";

        /// <summary>
        /// Custom labels to add to the logs sent to Loki.
        /// </summary>
        public IDictionary<string, string> Labels { get; set; } = new Dictionary<string, string>();
    }
}