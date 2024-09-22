namespace Paralax.Logging.Options
{
    public class ElkOptions
    {
        /// <summary>
        /// Enable or disable ELK logging.
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// The URI of the Elasticsearch server.
        /// </summary>
        public string Url { get; set; } = "http://localhost:9200";

        /// <summary>
        /// The name of the index to log to in Elasticsearch.
        /// </summary>
        public string IndexFormat { get; set; } = "logstash-{0:yyyy.MM.dd}";

        /// <summary>
        /// Specify the log level for ELK output.
        /// </summary>
        public string Level { get; set; } = "Information";
    }
}