using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Paralax.CQRS.Events;
using Paralax.CQRS.Events.Dispatchers;
using System.Collections.Generic;

namespace Paralax.CQRS.Events.Tests
{
    public class EventDispatcherTests
    {
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IServiceScope> _serviceScopeMock;
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IEventHandler<TestEvent>> _eventHandlerMock;

        public EventDispatcherTests()
        {
            _serviceProviderMock = new Mock<IServiceProvider>();
            _serviceScopeMock = new Mock<IServiceScope>();
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _eventHandlerMock = new Mock<IEventHandler<TestEvent>>();

            // Setup for service provider to create a scope
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
                .Returns(_serviceScopeFactoryMock.Object);

            _serviceScopeFactoryMock.Setup(f => f.CreateScope())
                .Returns(_serviceScopeMock.Object);

            // Setup to return the list of handlers
            var handlers = new List<IEventHandler<TestEvent>> { _eventHandlerMock.Object };
            _serviceScopeMock.Setup(s => s.ServiceProvider)
                .Returns(_serviceProviderMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(IEnumerable<IEventHandler<TestEvent>>)))
                .Returns(handlers);
        }

        [Fact]
        public async Task EventDispatcher_ShouldPublishEventToHandlers()
        {
            // Arrange
            var dispatcher = new EventDispatcher(_serviceProviderMock.Object);
            var testEvent = new TestEvent();

            // Act
            await dispatcher.PublishAsync(testEvent, CancellationToken.None);

            // Assert
            _eventHandlerMock.Verify(h => h.HandleAsync(testEvent, CancellationToken.None), Times.Once);
        }

        public class TestEvent : IEvent
        {
        }
    }
}
