namespace Paralax.Logging
{
    public interface ILoggingService
    {
        /// <summary>
        /// Sets the logging level for the application at runtime.
        /// </summary>
        /// <param name="logEventLevel">The log event level to set (e.g., Information, Warning, Error).</param>
        void SetLoggingLevel(string logEventLevel)
            => Extensions.LoggingLevelSwitch.MinimumLevel = Extensions.GetLogEventLevel(logEventLevel);
    }
}