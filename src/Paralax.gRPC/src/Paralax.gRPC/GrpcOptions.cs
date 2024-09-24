namespace Paralax.gRPC
{
    public class GrpcOptions
    {
        public int Port { get; set; } = 5001;
        public bool EnableReflection { get; set; } = true;
        public int MaxReceiveMessageSize { get; set; } = 4 * 1024 * 1024; 
        public int MaxSendMessageSize { get; set; } = 4 * 1024 * 1024; 
        public bool EnableRetries { get; set; } = true;
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
        public string ServiceName { get; set; } = "DefaultService";
        public string ServiceVersion { get; set; } = "1.0.0";
        public string Environment { get; set; } = "Production";
    }
}
