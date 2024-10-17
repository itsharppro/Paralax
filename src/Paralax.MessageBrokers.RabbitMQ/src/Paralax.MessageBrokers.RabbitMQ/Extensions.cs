using System;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Paralax.MessageBrokers.RabbitMQ.Clients;
using Paralax.MessageBrokers.RabbitMQ.Contexts;
using Paralax.MessageBrokers.RabbitMQ.Conventions;
using Paralax.MessageBrokers.RabbitMQ.Initializers;
using Paralax.MessageBrokers.RabbitMQ.Internals;
using Paralax.MessageBrokers.RabbitMQ.Plugins;
using Paralax.MessageBrokers.RabbitMQ.Publishers;
using Paralax.MessageBrokers.RabbitMQ.Serializers;
using Paralax.MessageBrokers.RabbitMQ.Subscribers;
using RabbitMQ.Client;

namespace Paralax.MessageBrokers.RabbitMQ
{
    public static class Extensions
    {
        private const string SectionName = "rabbitmq";
        private const string RegistryName = "messageBrokers.rabbitmq";

        public static IParalaxBuilder AddRabbitMq(this IParalaxBuilder builder, string sectionName = SectionName,
            Func<IRabbitMqPluginsRegistry, IRabbitMqPluginsRegistry> plugins = null,
            Action<ConnectionFactory> connectionFactoryConfigurator = null, IRabbitMqSerializer serializer = null)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = SectionName;
            }

            // Fetch the list of RabbitMqOptions (supports multiple configurations)
            var optionsList = builder.GetOptions<List<RabbitMqOptions>>(sectionName);

            if (optionsList == null || !optionsList.Any())
            {
                throw new ArgumentException("RabbitMQ configurations are not specified.");
            }

            // Iterate over each RabbitMQ configuration
            foreach (var options in optionsList)
            {
                builder.Services.AddSingleton(options);

                if (!builder.TryRegister(RegistryName))
                {
                    return builder;
                }

                if (options.HostNames == null || !options.HostNames.Any())
                {
                    throw new ArgumentException("RabbitMQ hostnames are not specified.", nameof(options.HostNames));
                }

                ILogger<IRabbitMqClient> logger;
                using (var serviceProvider = builder.Services.BuildServiceProvider())
                {
                    logger = serviceProvider.GetRequiredService<ILogger<IRabbitMqClient>>();
                }

                builder.Services.AddSingleton<IContextProvider, ContextProvider>();
                builder.Services.AddSingleton<ICorrelationContextAccessor>(new CorrelationContextAccessor());
                builder.Services.AddSingleton<IMessagePropertiesAccessor, MessagePropertiesAccessor>();
                builder.Services.AddSingleton<IConventionsBuilder, ConventionsBuilder>();
                builder.Services.AddSingleton<IConventionsProvider, ConventionsProvider>();
                builder.Services.AddSingleton<IConventionsRegistry, ConventionsRegistry>();
                builder.Services.AddSingleton<IRabbitMqClient, RabbitMqClient>();
                builder.Services.AddSingleton<IBusPublisher, RabbitMqPublisher>();
                builder.Services.AddSingleton<MessageSubscribersChannel>();
                builder.Services.AddSingleton<IBusSubscriber, RabbitMqSubscriber>();
                builder.Services.AddTransient<RabbitMqExchangeInitializer>();
                builder.Services.AddHostedService<RabbitMqBackgroundService>();
                builder.AddInitializer<RabbitMqExchangeInitializer>();

                // Add RabbitMQ properties and channel factories
                builder.Services.AddSingleton<IRabbitMqPropertiesFactory, RabbitMqPropertiesFactory>();
                builder.Services.AddSingleton<IRabbitMqChannelFactory, RabbitMqChannelFactory>();

                // Use custom or default serializer
                if (serializer != null)
                {
                    builder.Services.AddSingleton(serializer);
                }
                else
                {
                    builder.Services.AddSingleton<IRabbitMqSerializer, NetJsonRabbitMqSerializer>();
                }

                // Register RabbitMQ plugins
                var pluginsRegistry = new RabbitMqPluginsRegistry();
                builder.Services.AddSingleton<IRabbitMqPluginsRegistryAccessor>(pluginsRegistry);
                builder.Services.AddSingleton<IRabbitMqPluginsExecutor, RabbitMqPluginsExecutor>();
                plugins?.Invoke(pluginsRegistry);

                // Configure connection factory
                var connectionFactory = ConfigureConnectionFactory(options);
                ConfigureSsl(connectionFactory, options, logger);
                connectionFactoryConfigurator?.Invoke(connectionFactory);

                // Create connections for producers and consumers
                var consumerConnection = connectionFactory.CreateConnection(options.HostNames.ToList(), $"{options.ConnectionName}_consumer");
                var producerConnection = connectionFactory.CreateConnection(options.HostNames.ToList(), $"{options.ConnectionName}_producer");

                // Register connections as singletons
                builder.Services.AddSingleton(new ConsumerConnection(consumerConnection));
                builder.Services.AddSingleton(new ProducerConnection(producerConnection));

                // Register plugins as transient services
                ((IRabbitMqPluginsRegistryAccessor)pluginsRegistry).Get().ToList().ForEach(p =>
                    builder.Services.AddTransient(p.PluginType));
            }

            return builder;
        }


        private static ConnectionFactory ConfigureConnectionFactory(RabbitMqOptions options)
        {
            return new ConnectionFactory
            {
                Port = options.Port,
                VirtualHost = options.VirtualHost,
                UserName = options.Username,
                Password = options.Password,
                RequestedHeartbeat = options.RequestedHeartbeat,
                RequestedConnectionTimeout = options.RequestedConnectionTimeout,
                SocketReadTimeout = options.SocketReadTimeout,
                SocketWriteTimeout = options.SocketWriteTimeout,
                RequestedChannelMax = options.RequestedChannelMax,
                RequestedFrameMax = options.RequestedFrameMax,
                UseBackgroundThreadsForIO = options.UseBackgroundThreadsForIO,
                DispatchConsumersAsync = true,
                ContinuationTimeout = options.ContinuationTimeout,
                HandshakeContinuationTimeout = options.HandshakeContinuationTimeout,
                NetworkRecoveryInterval = options.NetworkRecoveryInterval,
                Ssl = options.Ssl == null
                    ? new SslOption()
                    : new SslOption(options.Ssl.ServerName, options.Ssl.CertificatePath, options.Ssl.Enabled)
            };
        }

        private static void ConfigureSsl(ConnectionFactory connectionFactory, RabbitMqOptions options, ILogger<IRabbitMqClient> logger)
        {
            if (options.Ssl == null || string.IsNullOrWhiteSpace(options.Ssl.ServerName))
            {
                connectionFactory.Ssl = new SslOption();
                return;
            }

            connectionFactory.Ssl = new SslOption(options.Ssl.ServerName, options.Ssl.CertificatePath, options.Ssl.Enabled);
            logger.LogDebug($"RabbitMQ SSL is: {(options.Ssl.Enabled ? "enabled" : "disabled")}, " +
                            $"server: '{options.Ssl.ServerName}', client certificate: '{options.Ssl.CertificatePath}', " +
                            $"CA certificate: '{options.Ssl.CaCertificatePath}'.");

            if (!string.IsNullOrWhiteSpace(options.Ssl.CaCertificatePath))
            {
                connectionFactory.Ssl.CertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
                {
                    if (sslPolicyErrors == SslPolicyErrors.None) return true;

                    chain = new X509Chain();
                    var certificate2 = new X509Certificate2(certificate);
                    var signerCertificate2 = new X509Certificate2(options.Ssl.CaCertificatePath);
                    chain.ChainPolicy.ExtraStore.Add(signerCertificate2);
                    chain.Build(certificate2);

                    var ignoredStatuses = options.Ssl.X509IgnoredStatuses?
                        .Select(s => Enum.Parse<X509ChainStatusFlags>(s, true)) ?? Enumerable.Empty<X509ChainStatusFlags>();

                    var isValid = chain.ChainStatus.All(chainStatus => chainStatus.Status == X509ChainStatusFlags.NoError ||
                                                                      ignoredStatuses.Contains(chainStatus.Status));
                    if (!isValid)
                    {
                        logger.LogError(string.Join(Environment.NewLine,
                            chain.ChainStatus.Select(s => $"{s.Status} - {s.StatusInformation}")));
                    }

                    return isValid;
                };
            }
        }

        public static IParalaxBuilder AddExceptionToMessageMapper<T>(this IParalaxBuilder builder)
            where T : class, IExceptionToMessageMapper
        {
            builder.Services.AddSingleton<IExceptionToMessageMapper, T>();
            return builder;
        }

        public static IParalaxBuilder AddExceptionToFailedMessageMapper<T>(this IParalaxBuilder builder)
            where T : class, IExceptionToFailedMessageMapper
        {
            builder.Services.AddSingleton<IExceptionToFailedMessageMapper, T>();
            return builder;
        }

        public static IBusSubscriber UseRabbitMq(this IApplicationBuilder app)
        {
            var messageSubscribersChannel = app.ApplicationServices.GetRequiredService<MessageSubscribersChannel>();

            var clients = app.ApplicationServices.GetService<IEnumerable<IRabbitMqClient>>() 
                        ?? new[] { app.ApplicationServices.GetRequiredService<IRabbitMqClient>() };

            return new RabbitMqSubscriber(clients, messageSubscribersChannel);
        }

    }
}
