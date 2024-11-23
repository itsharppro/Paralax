using System;
using System.Linq;
using System.Net.Http;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Paralax.gRPC.Builders;

namespace Paralax.gRPC
{
    public static class ClientExtensions
    {
        private const string GrpcClientOptionsSection = "GrpcClient";

        /// <summary>
        /// Registers a gRPC client with the provided options from configuration.
        /// </summary>
        /// <typeparam name="TClient">The gRPC client type to register.</typeparam>
        /// <param name="builder">The Paralax builder for service registration.</param>
        /// <param name="serviceName">The name of the service (key in the configuration) to look up.</param>
        /// <param name="sectionName">The configuration section containing gRPC client options.</param>
        /// <returns>The updated builder for chaining.</returns>
        public static IParalaxBuilder AddParalaxGrpcClient<TClient>(this IParalaxBuilder builder, string serviceName, string sectionName = GrpcClientOptionsSection) where TClient : class
        {
            var clientOptions = builder.GetOptions<GrpcClientOptions>(sectionName);

            if (clientOptions == null)
                throw new InvalidOperationException($"Configuration section '{sectionName}' is missing or invalid.");
                
            builder.Services.AddSingleton(clientOptions); 

            if (!clientOptions.Services.TryGetValue(serviceName, out var serviceAddress) || string.IsNullOrWhiteSpace(serviceAddress))
                throw new InvalidOperationException($"No configuration found for gRPC service '{serviceName}' in '{sectionName}'.");

            if (!Uri.TryCreate(serviceAddress, UriKind.Absolute, out var serviceUri))
                throw new ArgumentException($"The service address '{serviceAddress}' is not a valid URI.");

            builder.Services.AddGrpcClient<TClient>(options =>
            {
                options.Address = serviceUri;
            }).ConfigureChannel(channelOptions =>
            {
                channelOptions.MaxReceiveMessageSize = clientOptions.MaxReceiveMessageSize;
                channelOptions.MaxSendMessageSize = clientOptions.MaxSendMessageSize;
                channelOptions.HttpHandler = CreateHttpHandler(clientOptions);
                channelOptions.DisposeHttpClient = true;
            });

            return builder;
        }

        /// <summary>
        /// Creates an HTTP handler with SSL and retry options.
        /// </summary>
        /// <param name="options">The gRPC client options.</param>
        /// <returns>An HTTP handler.</returns>
        private static HttpMessageHandler CreateHttpHandler(GrpcClientOptions options)
        {
            return new SocketsHttpHandler
            {
                EnableMultipleHttp2Connections = true,
                SslOptions = new System.Net.Security.SslClientAuthenticationOptions
                {
                    RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
                        options.IgnoreCertificateErrors || sslPolicyErrors == System.Net.Security.SslPolicyErrors.None
                }
            };
        }
    }
}
