using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Paralax.CQRS.Commands;
using Paralax.CQRS.Commands.Dispatchers;
using Xunit;

namespace Paralax.CQRS.Commands.Tests
{
    public class CommandDispatcherTests
    {
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<ICommandHandler<TestCommand>> _commandHandlerMock;
        private readonly CommandDispatcher _dispatcher;

        public CommandDispatcherTests()
        {
            _serviceProviderMock = new Mock<IServiceProvider>();
            _commandHandlerMock = new Mock<ICommandHandler<TestCommand>>();

            var serviceScopeFactory = new Mock<IServiceScopeFactory>();
            var serviceScope = new Mock<IServiceScope>();
            serviceScope.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);
            serviceScopeFactory.Setup(s => s.CreateScope()).Returns(serviceScope.Object);

            _serviceProviderMock.Setup(sp => sp.GetService(typeof(ICommandHandler<TestCommand>)))
                .Returns(_commandHandlerMock.Object);

            _serviceProviderMock.Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
                .Returns(serviceScopeFactory.Object);

            _dispatcher = new CommandDispatcher(_serviceProviderMock.Object);
        }

        [Fact]
        public async Task DispatchAsync_Should_Call_Handler_When_Command_Is_Valid()
        {
            // Arrange
            var command = new TestCommand();

            // Act
            await _dispatcher.DispatchAsync(command);

            // Assert
            _commandHandlerMock.Verify(handler => handler.HandleAsync(command, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DispatchAsync_Should_Throw_Exception_When_Command_Is_Null()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _dispatcher.DispatchAsync<TestCommand>(null));
        }

        [Fact]
        public async Task DispatchAsync_Should_Throw_Exception_When_Handler_Not_Registered()
        {
            // Arrange
            var command = new UnhandledCommand();

            // Remove the command handler for UnhandledCommand
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(ICommandHandler<UnhandledCommand>)))
                .Returns(null as ICommandHandler<UnhandledCommand>);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _dispatcher.DispatchAsync(command));
        }

        // Sample commands
        public class TestCommand : ICommand { }
        public class UnhandledCommand : ICommand { }
    }
}
