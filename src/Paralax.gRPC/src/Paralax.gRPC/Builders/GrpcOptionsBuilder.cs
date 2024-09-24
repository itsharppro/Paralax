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

        public GrpcOptionsBuilder UsePort(int port)
        {
            _options.Port = port;
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