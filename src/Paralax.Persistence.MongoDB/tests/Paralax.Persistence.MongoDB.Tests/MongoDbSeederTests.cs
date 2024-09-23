using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Moq;
using Xunit;
using Paralax.Persistence.MongoDB.Seeders;
using MongoDB.Bson;

namespace Paralax.Persistence.MongoDB.Tests.Seeders
{
    public class MongoDbSeederTests
    {
        private readonly Mock<IMongoDatabase> _mongoDatabaseMock;
        private readonly Mock<IAsyncCursor<BsonDocument>> _cursorMock;
        private readonly MongoDbSeeder _mongoDbSeeder;

        public MongoDbSeederTests()
        {
            _mongoDatabaseMock = new Mock<IMongoDatabase>();
            _cursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            _mongoDbSeeder = new MongoDbSeeder();
        }

        [Fact]
        public async Task SeedAsync_Should_Seed_When_No_Collections_Exist()
        {
            // Arrange
            var emptyCollections = new List<BsonDocument>();

            // Setup the cursor to return an empty list
            _cursorMock
                .SetupSequence(cursor => cursor.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)  // MoveNextAsync returns true (collection query attempt)
                .ReturnsAsync(false); // No more results after the first call

            _cursorMock
                .SetupGet(cursor => cursor.Current)
                .Returns(emptyCollections);

            // Setup the ListCollectionsAsync to return the mock cursor
            _mongoDatabaseMock
                .Setup(db => db.ListCollectionsAsync(It.IsAny<ListCollectionsOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_cursorMock.Object);

            // Act
            await _mongoDbSeeder.SeedAsync(_mongoDatabaseMock.Object);

            // Assert
            _mongoDatabaseMock.Verify(db => db.ListCollectionsAsync(It.IsAny<ListCollectionsOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            _cursorMock.Verify(cursor => cursor.MoveNextAsync(It.IsAny<CancellationToken>()), Times.Exactly(2)); // Called twice: once for query, once for no more results
        }

        [Fact]
        public async Task SeedAsync_Should_Not_Seed_When_Collections_Exist()
        {
            // Arrange
            var existingCollections = new List<BsonDocument> { new BsonDocument("name", "existingCollection") };

            // Setup the cursor to return a list with an existing collection
            _cursorMock
                .SetupSequence(cursor => cursor.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _cursorMock
                .SetupGet(cursor => cursor.Current)
                .Returns(existingCollections);

            // Setup the ListCollectionsAsync to return the mock cursor
            _mongoDatabaseMock
                .Setup(db => db.ListCollectionsAsync(It.IsAny<ListCollectionsOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_cursorMock.Object);

            // Act
            await _mongoDbSeeder.SeedAsync(_mongoDatabaseMock.Object);

            // Assert
            _mongoDatabaseMock.Verify(db => db.ListCollectionsAsync(It.IsAny<ListCollectionsOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            _cursorMock.Verify(cursor => cursor.MoveNextAsync(It.IsAny<CancellationToken>()), Times.Exactly(2)); // Called twice
        }
    }
}
