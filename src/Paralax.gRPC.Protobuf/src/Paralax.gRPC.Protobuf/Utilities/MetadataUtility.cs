

using Paralax.Common;

namespace Paralax.gRPC.Protobuf.Utilities
{
    public static class MetadataUtility
    {
        public static Metadata CreateMetadata(string serviceName, string requestId = "")
        {
            return new Metadata
            {
                RequestId = string.IsNullOrWhiteSpace(requestId) ? Guid.NewGuid().ToString() : requestId,
                Timestamp = DateTime.UtcNow.ToString("o"),
                ServiceName = serviceName
            };
        }
    }
}