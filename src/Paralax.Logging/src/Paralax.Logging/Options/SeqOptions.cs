namespace Paralax.Logging.Options
{
    public class SeqOptions
    {
        /// <summary>
        /// Enable or disable Seq logging.
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// The URI of the Seq server.
        /// </summary>
        public string Url { get; set; } = "http://localhost:5341";

        /// <summary>
        /// Specify the log level for Seq output.
        /// </summary>
        public string Level { get; set; } = "Information";

        /// <summary>
        /// API key for authenticating with Seq, if needed.
        /// </summary>
        public string ApiKey { get; set; }
    }
}