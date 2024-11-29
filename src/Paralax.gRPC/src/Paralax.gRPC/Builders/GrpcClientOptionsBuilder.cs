using System;
using System.Collections.Generic;

namespace Paralax.gRPC.Builders
{
    public class GrpcClientOptionsBuilder
    {
        private readonly GrpcClientOptions _options;

        public GrpcClientOptionsBuilder()
        {
            _options = new GrpcClientOptions();
        }

        /// <summary>
        /// Adds a service to the gRPC client options.
        /// </summary>
        /// <param name="name">The name of the service (e.g., "tracking-collector").</param>
        /// <param name="address">The base address of the service (e.g., "https://localhost:7146").</param>
        /// <returns>The builder for chaining.</returns>
        public GrpcClientOptionsBuilder AddService(string name, string address)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Service name cannot be null or empty.", nameof(name));

            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Service address cannot be null or empty.", nameof(address));

            _options.Services[name] = address;
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

        public GrpcClientOptionsBuilder SetTimeout(TimeSpan timeout)
        {
            _options.Timeout = timeout;
            return this;
        }

        public GrpcClientOptionsBuilder EnableRetries(bool enable)
        {
            _options.EnableRetries = enable;
            return this;
        }

        public GrpcClientOptionsBuilder IgnoreCertificateErrors(bool ignore)
        {
            _options.IgnoreCertificateErrors = ignore;
            return this;
        }

        public GrpcClientOptions Build()
        {
            return _options;
        }
    }
}
