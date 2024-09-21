namespace Paralax.Logging.Options
{
    public class ConsoleOptions
    {
        /// <summary>
        /// Enable or disable console logging.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Specify the log level for console output (e.g., Information, Warning, Error).
        /// </summary>
        public string Level { get; set; } = "Information";

        /// <summary>
        /// Format of the console output (e.g., JSON or plain text).
        /// </summary>
        public string OutputTemplate { get; set; } = "[{Timestamp:HH:mm:ss} {Level}] {Message}{NewLine}{Exception}";
    }
}