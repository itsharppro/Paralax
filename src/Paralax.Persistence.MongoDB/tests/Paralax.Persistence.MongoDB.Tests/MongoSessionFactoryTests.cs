using System.Threading.Tasks;
using MongoDB.Driver;
using Moq;
using Xunit;
using Paralax.Persistence.MongoDB;
using Paralax.Persistence.MongoDB.Factories;

namespace Paralax.Persistence.MongoDB.Tests
{
    public class MongoSessionFactoryTests
    {
        private readonly Mock<IMongoClient> _mongoClientMock;
        private readonly Mock<IClientSessionHandle> _clientSessionHandleMock;
        private readonly MongoSessionFactory _mongoSessionFactory;

        public MongoSessionFactoryTests()
        {
            // Create mocks for the IMongoClient and IClientSessionHandle
            _mongoClientMock = new Mock<IMongoClient>();
            _clientSessionHandleMock = new Mock<IClientSessionHandle>();

            // Setup the StartSessionAsync method to return the mock session
            _mongoClientMock
                .Setup(client => client.StartSessionAsync(It.IsAny<ClientSessionOptions>(), default))
                .ReturnsAsync(_clientSessionHandleMock.Object);

            // Create an instance of MongoSessionFactory using the mocked client
            _mongoSessionFactory = new MongoSessionFactory(_mongoClientMock.Object);
        }

        [Fact]
        public async Task CreateAsync_Should_Return_ClientSessionHandle()
        {
            // Act
            var session = await _mongoSessionFactory.CreateAsync();

            // Assert
            Assert.NotNull(session);
            Assert.Equal(_clientSessionHandleMock.Object, session);

            // Verify that StartSessionAsync was called exactly once
            _mongoClientMock.Verify(client => client.StartSessionAsync(It.IsAny<ClientSessionOptions>(), default), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_Should_Call_StartSessionAsync_On_MongoClient()
        {
            // Act
            await _mongoSessionFactory.CreateAsync();

            // Assert
            _mongoClientMock.Verify(client => client.StartSessionAsync(It.IsAny<ClientSessionOptions>(), default), Times.Once);
        }
    }
}
