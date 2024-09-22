using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Paralax;
using Paralax.CQRS.Commands;
using Paralax.CQRS.Commands.Dispatchers;
using Xunit;
using System.Threading.Tasks;
using System.Threading;

namespace Paralax.CQRS.Commands.Tests
{
    public class ParalaxBuilderExtensionsTests
    {
        private readonly IServiceCollection _serviceCollection;
        private readonly IParalaxBuilder _paralaxBuilder;

        public ParalaxBuilderExtensionsTests()
        {
            // Use the real IServiceCollection and ParalaxBuilder
            _serviceCollection = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build(); // Mock or real configuration can be added here
            _paralaxBuilder = ParalaxBuilder.Create(_serviceCollection, configuration);

            // Register a dummy ICommandHandler<> implementation for testing purposes
            _serviceCollection.AddTransient<ICommandHandler<TestCommand>, TestCommandHandler>();
        }

        [Fact]
        public void AddCommandHandlers_Should_Register_CommandHandlers_AsTransient()
        {
            // Act
            var result = _paralaxBuilder.AddCommandHandlers();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_paralaxBuilder, result);

            // Verify that command handlers are registered with Transient lifetime
            var commandHandlers = _serviceCollection
                .Where(d => d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() == typeof(ICommandHandler<>))
                .ToList();

            Assert.NotEmpty(commandHandlers); // Ensure we have command handlers registered
            Assert.All(commandHandlers, descriptor =>
            {
                Assert.Equal(ServiceLifetime.Transient, descriptor.Lifetime);
            });
        }

        [Fact]
        public void AddInMemoryCommandDispatcher_Should_Register_CommandDispatcher_AsSingleton()
        {
            // Act
            var result = _paralaxBuilder.AddInMemoryCommandDispatcher();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_paralaxBuilder, result);

            // Verify that ICommandDispatcher is registered as a singleton
            var descriptor = _serviceCollection.FirstOrDefault(d => d.ServiceType == typeof(ICommandDispatcher));
            Assert.NotNull(descriptor);
            Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
            Assert.Equal(typeof(CommandDispatcher), descriptor.ImplementationType);
        }

        // Mock command and command handler for testing purposes
        public class TestCommand : ICommand { }

        public class TestCommandHandler : ICommandHandler<TestCommand>
        {
            public Task HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }
        }
    }
}
