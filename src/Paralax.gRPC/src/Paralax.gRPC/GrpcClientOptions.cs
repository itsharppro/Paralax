namespace Paralax.gRPC
{
    public class GrpcClientOptions
    {
        public IDictionary<Type, string> Services { get; set; } = new Dictionary<Type, string>();
        public int MaxReceiveMessageSize { get; set; } = 4 * 1024 * 1024;
        public int MaxSendMessageSize { get; set; } = 4 * 1024 * 1024;
    }
}
