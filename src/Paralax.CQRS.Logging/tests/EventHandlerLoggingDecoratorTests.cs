using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Paralax.CQRS.Events;
using Xunit;

namespace Paralax.CQRS.Logging.Decorators.Tests
{
    public class EventHandlerLoggingDecoratorTests
    {
        private readonly Mock<IEventHandler<TestEvent>> _handlerMock;
        private readonly Mock<ILogger<EventHandlerLoggingDecorator<TestEvent>>> _loggerMock;
        private readonly Mock<IMessageToLogTemplateMapper> _mapperMock;
        private readonly EventHandlerLoggingDecorator<TestEvent> _decorator;

        public EventHandlerLoggingDecoratorTests()
        {
            _handlerMock = new Mock<IEventHandler<TestEvent>>();
            _loggerMock = new Mock<ILogger<EventHandlerLoggingDecorator<TestEvent>>>();
            _mapperMock = new Mock<IMessageToLogTemplateMapper>();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IMessageToLogTemplateMapper)))
                .Returns(_mapperMock.Object);

            _decorator = new EventHandlerLoggingDecorator<TestEvent>(
                _handlerMock.Object, 
                _loggerMock.Object, 
                serviceProviderMock.Object
            );
        }

        [Fact]
        public async Task HandleAsync_ShouldLogBeforeAndAfter_WhenTemplateIsProvided()
        {
            // Arrange
            var @event = new TestEvent();
            var template = new HandlerLogTemplate
            {
                Before = "Before handling event: {Id}",
                After = "After handling event: {Id}"
            };

            _mapperMock.Setup(m => m.Map(@event)).Returns(template);

            // Act
            await _decorator.HandleAsync(@event);

            // Assert
            _loggerMock.Verify(
                logger => logger.Log(
                    It.Is<LogLevel>(level => level == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Before handling event:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ), Times.Once);

            _handlerMock.Verify(handler => handler.HandleAsync(@event, It.IsAny<CancellationToken>()), Times.Once);

            _loggerMock.Verify(
                logger => logger.Log(
                    It.Is<LogLevel>(level => level == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("After handling event:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_ShouldNotLog_WhenTemplateIsNull()
        {
            // Arrange
            var @event = new TestEvent();

            _mapperMock.Setup(m => m.Map(@event)).Returns((HandlerLogTemplate)null);

            // Act
            await _decorator.HandleAsync(@event);

            // Assert
            _loggerMock.VerifyNoOtherCalls(); // No logging happens
            _handlerMock.Verify(handler => handler.HandleAsync(@event, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_ShouldLogError_WhenExceptionIsThrown()
        {
            // Arrange
            var @event = new TestEvent();
            var template = new HandlerLogTemplate
            {
                Before = "Before handling event: {Id}",
                After = "After handling event: {Id}",
                OnError = new Dictionary<Type, string>
                {
                    { typeof(InvalidOperationException), "Error during event handling: {Id}" }
                }
            };

            _mapperMock.Setup(m => m.Map(@event)).Returns(template);

            _handlerMock
                .Setup(handler => handler.HandleAsync(@event, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _decorator.HandleAsync(@event));

            _loggerMock.Verify(
                logger => logger.Log(
                    It.Is<LogLevel>(level => level == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Before handling event:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ), Times.Once);

            _loggerMock.Verify(
                logger => logger.Log(
                    It.Is<LogLevel>(level => level == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error during event handling:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_ShouldThrowException_WhenHandlerThrowsException()
        {
            // Arrange
            var @event = new TestEvent();
            var template = new HandlerLogTemplate
            {
                Before = "Before handling event: {Id}",
                After = "After handling event: {Id}"
            };

            _mapperMock.Setup(m => m.Map(@event)).Returns(template);

            _handlerMock
                .Setup(handler => handler.HandleAsync(@event, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException());

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _decorator.HandleAsync(@event));

            // Ensure that exception propagates properly
            _handlerMock.Verify(handler => handler.HandleAsync(@event, It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    // Test event class
    public class TestEvent : IEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
    }
}
