using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Paralax.CQRS.Commands;
using Paralax.CQRS.Events;
using Paralax.CQRS.Logging;
using Paralax.CQRS.Logging.Decorators;
using Xunit;

namespace Paralax.CQRS.Logging.Tests
{
    public class ExtensionsTests
    {
        private readonly Mock<IServiceCollection> _servicesMock;
        private readonly Mock<IParalaxBuilder> _builderMock;

        public ExtensionsTests()
        {
            _servicesMock = new Mock<IServiceCollection>();
            _builderMock = new Mock<IParalaxBuilder>();

            _builderMock.SetupGet(b => b.Services).Returns(_servicesMock.Object);
        }

        [Fact]
        public void AddCommandHandlersLogging_ShouldAddLoggingDecoratorForCommandHandlers()
        {
            // Arrange
            var assembly = Assembly.GetExecutingAssembly(); // Use current assembly for testing

            // Act
            var result = _builderMock.Object.AddCommandHandlersLogging(assembly);

            // Assert
            _builderMock.VerifyGet(builder => builder.Services, Times.Once);  // Verify that 'Services' was accessed
            _builderMock.VerifyNoOtherCalls();  // Verify that no other methods were called on the builder
            Assert.NotNull(result); // Ensure that it returns the builder itself
        }

        [Fact]
        public void AddEventHandlersLogging_ShouldAddLoggingDecoratorForEventHandlers()
        {
            // Arrange
            var assembly = Assembly.GetExecutingAssembly(); // Use current assembly for testing

            // Act
            var result = _builderMock.Object.AddEventHandlersLogging(assembly);

            // Assert
            _builderMock.VerifyGet(builder => builder.Services, Times.Once);  // Verify that 'Services' was accessed
            _builderMock.VerifyNoOtherCalls();  // Verify that no other methods were called on the builder
            Assert.NotNull(result); // Ensure that it returns the builder itself
        }
    }
}
