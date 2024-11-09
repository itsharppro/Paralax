using System;

namespace Paralax.gRPC.Builders
{
    public class GrpcOptionsBuilder
    {
        private readonly GrpcOptions _options;

        public GrpcOptionsBuilder()
        {
            _options = new GrpcOptions();
        }

        // Set the port for REST API (HTTP/1.1)
        public GrpcOptionsBuilder UseRestPort(int restPort)
        {
            _options.RestPort = restPort;
            return this;
        }

        // Set the port for gRPC service (HTTP/2)
        public GrpcOptionsBuilder UseGrpcPort(int grpcPort)
        {
            _options.GrpcPort = grpcPort;
            return this;
        }

        public GrpcOptionsBuilder EnableReflection(bool enable)
        {
            _options.EnableReflection = enable;
            return this;
        }

        public GrpcOptionsBuilder SetMaxReceiveMessageSize(int sizeInBytes)
        {
            _options.MaxReceiveMessageSize = sizeInBytes;
            return this;
        }

        public GrpcOptionsBuilder SetMaxSendMessageSize(int sizeInBytes)
        {
            _options.MaxSendMessageSize = sizeInBytes;
            return this;
        }

        public GrpcOptionsBuilder EnableRetries(bool enable)
        {
            _options.EnableRetries = enable;
            return this;
        }

        public GrpcOptionsBuilder SetTimeout(TimeSpan timeout)
        {
            _options.Timeout = timeout;
            return this;
        }

        public GrpcOptionsBuilder SetServiceName(string serviceName)
        {
            _options.ServiceName = serviceName;
            return this;
        }

        public GrpcOptionsBuilder SetServiceVersion(string serviceVersion)
        {
            _options.ServiceVersion = serviceVersion;
            return this;
        }

        public GrpcOptionsBuilder SetEnvironment(string environment)
        {
            _options.Environment = environment;
            return this;
        }

        public GrpcOptions Build()
        {
            return _options;
        }
    }
}
