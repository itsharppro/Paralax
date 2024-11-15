using System;

namespace Paralax.gRPC.Builders
{
    public class GrpcClientOptionsBuilder
    {
        private readonly GrpcClientOptions _options;

        public GrpcClientOptionsBuilder()
        {
            _options = new GrpcClientOptions();
        }

        public GrpcClientOptionsBuilder AddService<TService>(string address)
        {
            _options.Services[typeof(TService)] = address;
            return this;
        }

        public GrpcClientOptionsBuilder SetMaxReceiveMessageSize(int sizeInBytes)
        {
            _options.MaxReceiveMessageSize = sizeInBytes;
            return this;
        }

        public GrpcClientOptionsBuilder SetMaxSendMessageSize(int sizeInBytes)
        {
            _options.MaxSendMessageSize = sizeInBytes;
            return this;
        }

        public GrpcClientOptions Build()
        {
            return _options;
        }
    }
}
