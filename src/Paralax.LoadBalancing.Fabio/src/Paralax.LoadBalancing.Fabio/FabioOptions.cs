namespace Paralax.LoadBalancing.Fabio
{
    public class FabioOptions
    {
        public bool Enabled { get; set; }
        public string Url { get; set; }
        public string Service { get; set; }        
        public int TimeoutMilliseconds { get; set; } = 5000; // Default 5 seconds
        public int RetryCount { get; set; } = 3; // Default 3 retries
        public bool HealthCheckEnabled { get; set; } = true; // Enable health check by default
        public string HealthCheckPath { get; set; } = "/health"; // Default health check endpoint
        public Dictionary<string, string> CustomHeaders { get; set; } = new Dictionary<string, string>(); // For custom headers
        public bool CircuitBreakerEnabled { get; set; } = false; // Disable by default
        public int CircuitBreakerThreshold { get; set; } = 5; // Default threshold for circuit breaker
    }
}
