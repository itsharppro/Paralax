using System;
using System.Collections.Generic;

namespace Paralax.gRPC
{
    public class GrpcClientOptions
    {
        public Dictionary<string, string> Services { get; set; } = new Dictionary<string, string>(); // Key: Service name, Value: Address
        public int MaxReceiveMessageSize { get; set; } = 4 * 1024 * 1024; // Default: 4 MB
        public int MaxSendMessageSize { get; set; } = 4 * 1024 * 1024;    // Default: 4 MB
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(1); // Default: 1 min
        public bool EnableRetries { get; set; } = false;                 // Default: no retries
        public bool IgnoreCertificateErrors { get; set; } = false;       // Default: validate certificates
    }
}
