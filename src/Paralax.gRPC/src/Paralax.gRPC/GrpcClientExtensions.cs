using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Routing;
using Paralax.gRPC.Builders;
// using Paralax.gRPC.Protobuf;
// using Paralax.gRPC.Protobuf.Utilities;
// using Paralax.Health;
using System.Diagnostics;
using Paralax.gRPC.Utils;
using Grpc.Net.Client;
using Grpc.Core;

namespace Paralax.gRPC
{
    public static class GrpcClientExtensions
    {
        /// <summary>
        /// Adds gRPC clients to the IServiceCollection based on the configuration in GrpcClientOptions.
        /// </summary>
        /// <param name="services">The service collection to add the clients to.</param>
        /// <param name="configuration">The configuration instance to bind options from.</param>
        /// <param name="sectionName">The configuration section name (default is 'GrpcClientOptions').</param>
        /// <returns>The IServiceCollection for chaining.</returns>
        public static IServiceCollection AddGrpcClients(
            this IServiceCollection services, 
            IConfiguration configuration, 
            string sectionName = "GrpcClientOptions")
        {
            // Load options from configuration
            var grpcClientOptions = new GrpcClientOptions();
            configuration.GetSection(sectionName).Bind(grpcClientOptions);

            foreach (var serviceEntry in grpcClientOptions.Services)
            {
                services.AddTransient(serviceEntry.Key, serviceProvider =>
                {
                    // Create a gRPC channel for each service
                    var channel = GrpcChannel.ForAddress(serviceEntry.Value, new GrpcChannelOptions
                    {
                        MaxReceiveMessageSize = grpcClientOptions.MaxReceiveMessageSize,
                        MaxSendMessageSize = grpcClientOptions.MaxSendMessageSize,
                        Credentials = ChannelCredentials.Insecure // Use secure credentials in production
                    });

                    // Instantiate the gRPC client with the channel
                    return Activator.CreateInstance(serviceEntry.Key, channel);
                });
            }

            return services;
        }

        /// <summary>
        /// Adds gRPC clients with programmatic configuration using GrpcClientOptionsBuilder.
        /// </summary>
        /// <param name="services">The service collection to add the clients to.</param>
        /// <param name="configure">A configuration action to set up the GrpcClientOptionsBuilder.</param>
        /// <returns>The IServiceCollection for chaining.</returns>
        public static IServiceCollection AddGrpcClients(
            this IServiceCollection services, 
            Action<GrpcClientOptionsBuilder> configure)
        {
            // Build GrpcClientOptions using the builder
            var builder = new GrpcClientOptionsBuilder();
            configure(builder);
            var grpcClientOptions = builder.Build();

            foreach (var serviceEntry in grpcClientOptions.Services)
            {
                services.AddTransient(serviceEntry.Key, serviceProvider =>
                {
                    // Create a gRPC channel for each service
                    var channel = GrpcChannel.ForAddress(serviceEntry.Value, new GrpcChannelOptions
                    {
                        MaxReceiveMessageSize = grpcClientOptions.MaxReceiveMessageSize,
                        MaxSendMessageSize = grpcClientOptions.MaxSendMessageSize,
                        Credentials = ChannelCredentials.Insecure // Use secure credentials in production
                    });

                    // Instantiate the gRPC client with the channel
                    return Activator.CreateInstance(serviceEntry.Key, channel);
                });
            }

            return services;
        }
    }
}
