namespace Paralax.Logging.Options
{
    public class FileLoggingOptions
    {
        /// <summary>
        /// Enable or disable file logging.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// The path to the log file.
        /// </summary>
        public string Path { get; set; } = "logs/paralax.log";

        /// <summary>
        /// Specify the log level for file output.
        /// </summary>
        public string Level { get; set; } = "Information";

        /// <summary>
        /// Maximum size of a log file before rolling over to a new file.
        /// </summary>
        public long FileSizeLimitBytes { get; set; } = 10 * 1024 * 1024; // 10 MB

        /// <summary>
        /// Number of log files to retain.
        /// </summary>
        public int RetainedFileCountLimit { get; set; } = 7;

        /// <summary>
        /// Enable or disable rolling file logging.
        /// </summary>
        public bool RollingFileEnabled { get; set; } = true;
    }
}