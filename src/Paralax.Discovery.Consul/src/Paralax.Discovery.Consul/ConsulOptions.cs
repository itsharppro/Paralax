using System.Collections.Generic;

namespace Paralax.Discovery.Consul
{
    public class ConsulOptions
    {
        // Enable or disable Consul integration
        public bool Enabled { get; set; }

        // The URL of the Consul agent or server
        public string Url { get; set; }

        // The name of the service to be registered in Consul
        public string Service { get; set; }

        // The address of the service to be registered
        public string Address { get; set; }

        // The port of the service to be registered
        public int Port { get; set; }

        // Enable or disable health check ping for the service
        public bool PingEnabled { get; set; }

        // The endpoint to be pinged for health checks
        public string PingEndpoint { get; set; }

        // Interval between health check pings (e.g., "10s" for 10 seconds)
        public string PingInterval { get; set; }

        // Time after which a failing service should be deregistered
        public string RemoveAfterInterval { get; set; }

        // Tags associated with the service for service discovery and filtering
        public List<string> Tags { get; set; } = new List<string>();

        // Metadata associated with the service for additional service details
        public IDictionary<string, string> Meta { get; set; } = new Dictionary<string, string>();

        // Enable overriding tags for the service in Consul
        public bool EnableTagOverride { get; set; }

        // Option to skip replacing "localhost" DNS for Docker in certain environments
        public bool SkipLocalhostDockerDnsReplace { get; set; }

        // Consul Connect options for service-to-service security
        public ConnectOptions Connect { get; set; } = new ConnectOptions();

        // Option to define a retry policy for connecting to Consul (e.g., 3 attempts)
        public int MaxRetryAttempts { get; set; } = 3;

        // Time between retry attempts (e.g., "5s" for 5 seconds)
        public string RetryDelay { get; set; } = "5s";

        // Time to wait for the service registration to complete (e.g., "30s" for 30 seconds)
        public string RegistrationTimeout { get; set; } = "30s";

        // Enable automatic deregistration when the service goes down
        public bool AutoDeregister { get; set; } = true;

        // The time-to-live (TTL) for service checks in Consul
        public string CheckTTL { get; set; } = "15s";

        // Enable or disable TLS verification for secure connections with Consul
        public bool EnableTlsVerification { get; set; } = false;

        // The path to the TLS certificate to use for secure connections
        public string TlsCertPath { get; set; }

        // Option to provide additional security parameters, like ACL tokens, for Consul
        public string AclToken { get; set; }

        // Enable or disable debugging for Consul-related operations
        public bool Debug { get; set; } = false;

        // Nested class for Consul Connect options
        public class ConnectOptions
        {
            // Enable or disable Consul Connect for service-to-service encryption
            public bool Enabled { get; set; }
        }
    }
}
