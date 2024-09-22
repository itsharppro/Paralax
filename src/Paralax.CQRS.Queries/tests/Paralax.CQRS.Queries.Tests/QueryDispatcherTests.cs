using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Paralax.CQRS.Queries;
using Paralax.CQRS.Queries.Dispatchers;
using Xunit;

namespace Paralax.CQRS.Queries.Tests
{
    public class QueryDispatcherTests
    {
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IQueryHandler<TestQuery, TestResult>> _queryHandlerMock;
        private readonly QueryDispatcher _dispatcher;

        public QueryDispatcherTests()
        {
            _serviceProviderMock = new Mock<IServiceProvider>();
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _queryHandlerMock = new Mock<IQueryHandler<TestQuery, TestResult>>();

            var serviceScope = new Mock<IServiceScope>();
            serviceScope.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);
            _serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScope.Object);

            _serviceProviderMock.Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
                .Returns(_serviceScopeFactoryMock.Object);

            _serviceProviderMock.Setup(sp => sp.GetService(typeof(IQueryHandler<TestQuery, TestResult>)))
                .Returns(_queryHandlerMock.Object);

            _dispatcher = new QueryDispatcher(_serviceProviderMock.Object);
        }

        [Fact]
        public async Task QueryAsync_Should_Invoke_QueryHandler_With_Correct_Query()
        {
            // Arrange
            var query = new TestQuery { QueryText = "Sample" };
            var expectedResult = new TestResult { ResultText = "Result" };
            _queryHandlerMock.Setup(qh => qh.HandleAsync(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _dispatcher.QueryAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult.ResultText, result.ResultText);
            _queryHandlerMock.Verify(qh => qh.HandleAsync(query, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task QueryAsync_Should_Throw_Exception_When_No_Handler_Found()
        {
            // Arrange
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(IQueryHandler<UnhandledQuery, TestResult>)))
                .Returns(null); // No handler for UnhandledQuery

            var unhandledQuery = new UnhandledQuery();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _dispatcher.QueryAsync<UnhandledQuery, TestResult>(unhandledQuery));
        }

        // Sample test query
        public class TestQuery : IQuery<TestResult>
        {
            public string QueryText { get; set; }
        }

        // Sample test result
        public class TestResult
        {
            public string ResultText { get; set; }
        }

        // Query without a handler for testing missing handler scenario
        public class UnhandledQuery : IQuery<TestResult>
        {
        }
    }
}
