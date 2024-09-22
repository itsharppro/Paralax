using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Paralax.CQRS.Commands;
using Xunit;

namespace Paralax.CQRS.Logging.Decorators.Tests
{
    public class CommandHandlerLoggingDecoratorTests
    {
        private readonly Mock<ICommandHandler<TestCommand>> _handlerMock;
        private readonly Mock<ILogger<CommandHandlerLoggingDecorator<TestCommand>>> _loggerMock;
        private readonly Mock<IMessageToLogTemplateMapper> _mapperMock;
        private readonly CommandHandlerLoggingDecorator<TestCommand> _decorator;

        public CommandHandlerLoggingDecoratorTests()
        {
            _handlerMock = new Mock<ICommandHandler<TestCommand>>();
            _loggerMock = new Mock<ILogger<CommandHandlerLoggingDecorator<TestCommand>>>();
            _mapperMock = new Mock<IMessageToLogTemplateMapper>();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IMessageToLogTemplateMapper)))
                .Returns(_mapperMock.Object);

            _decorator = new CommandHandlerLoggingDecorator<TestCommand>(
                _handlerMock.Object, 
                _loggerMock.Object, 
                serviceProviderMock.Object
            );
        }

        [Fact]
        public async Task HandleAsync_ShouldLogBeforeAndAfter_WhenTemplateIsProvided()
        {
            // Arrange
            var command = new TestCommand();
            var template = new HandlerLogTemplate
            {
                Before = "Before handling command: {Id}",
                After = "After handling command: {Id}"
            };

            _mapperMock.Setup(m => m.Map(command)).Returns(template);

            // Act
            await _decorator.HandleAsync(command);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Before handling command: " + command.Id),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once
            );

            _handlerMock.Verify(handler => handler.HandleAsync(command, It.IsAny<CancellationToken>()), Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "After handling command: " + command.Id),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once
            );
        }

        [Fact]
        public async Task HandleAsync_ShouldNotLog_WhenTemplateIsNull()
        {
            // Arrange
            var command = new TestCommand();

            _mapperMock.Setup(m => m.Map(command)).Returns((HandlerLogTemplate)null);

            // Act
            await _decorator.HandleAsync(command);

            // Assert
            _loggerMock.VerifyNoOtherCalls(); // No logging happens
            _handlerMock.Verify(handler => handler.HandleAsync(command, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_ShouldLogError_WhenExceptionIsThrown()
        {
            // Arrange
            var command = new TestCommand();
            var template = new HandlerLogTemplate
            {
                Before = "Before handling command: {Id}",
                After = "After handling command: {Id}",
                OnError = new Dictionary<Type, string>
                {
                    { typeof(InvalidOperationException), "Error during command handling: {Id}" }
                }
            };

            _mapperMock.Setup(m => m.Map(command)).Returns(template);

            _handlerMock
                .Setup(handler => handler.HandleAsync(command, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _decorator.HandleAsync(command));

            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Before handling command: " + command.Id),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once
            );

            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Error during command handling: " + command.Id),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once
            );
        }

        [Fact]
        public async Task HandleAsync_ShouldThrowException_WhenHandlerThrowsException()
        {
            // Arrange
            var command = new TestCommand();
            var template = new HandlerLogTemplate
            {
                Before = "Before handling command: {Id}",
                After = "After handling command: {Id}"
            };

            _mapperMock.Setup(m => m.Map(command)).Returns(template);

            _handlerMock
                .Setup(handler => handler.HandleAsync(command, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException());

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _decorator.HandleAsync(command));

            // Ensure that exception propagates properly
            _handlerMock.Verify(handler => handler.HandleAsync(command, It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    // Test command class
    public class TestCommand : ICommand
    {
        public Guid Id { get; set; } = Guid.NewGuid();
    }
}
