using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Moq;
using Xunit;
using Paralax.Persistence.MongoDB.Initializers;
using System.Reflection;

namespace Paralax.Persistence.MongoDB.Tests
{
    public class MongoDbInitializerTests
    {
        private readonly Mock<IMongoDatabase> _mongoDatabaseMock;
        private readonly Mock<IMongoDbSeeder> _mongoDbSeederMock;
        private readonly MongoDbOptions _mongoDbOptions;
        private MongoDbInitializer _mongoDbInitializer;

        public MongoDbInitializerTests()
        {
            _mongoDatabaseMock = new Mock<IMongoDatabase>();
            _mongoDbSeederMock = new Mock<IMongoDbSeeder>();
            _mongoDbOptions = new MongoDbOptions
            {
                Seed = true
            };

            _mongoDbInitializer = new MongoDbInitializer(
                _mongoDatabaseMock.Object,
                _mongoDbSeederMock.Object,
                _mongoDbOptions);
        }

        private void ResetStaticField()
        {
            var field = typeof(MongoDbInitializer).GetField("_initialized", BindingFlags.NonPublic | BindingFlags.Static);
            field.SetValue(null, 0);
        }

        [Fact]
        public async Task InitializeAsync_Should_SeedDatabase_When_SeedOptionIsTrue()
        {
            // Arrange
            ResetStaticField();

            // Act
            await _mongoDbInitializer.InitializeAsync();

            // Assert
            _mongoDbSeederMock.Verify(seeder => seeder.SeedAsync(_mongoDatabaseMock.Object), Times.Once);
        }

        [Fact]
        public async Task InitializeAsync_Should_NotSeedDatabase_When_SeedOptionIsFalse()
        {
            // Arrange
            ResetStaticField();
            _mongoDbOptions.Seed = false;
            _mongoDbInitializer = new MongoDbInitializer(
                _mongoDatabaseMock.Object,
                _mongoDbSeederMock.Object,
                _mongoDbOptions);

            // Act
            await _mongoDbInitializer.InitializeAsync();

            // Assert
            _mongoDbSeederMock.Verify(seeder => seeder.SeedAsync(It.IsAny<IMongoDatabase>()), Times.Never);
        }

        [Fact]
        public async Task InitializeAsync_Should_OnlyInitializeOnce()
        {
            // Arrange
            ResetStaticField();

            // Act
            await _mongoDbInitializer.InitializeAsync();
            await _mongoDbInitializer.InitializeAsync(); 

            // Assert
            _mongoDbSeederMock.Verify(seeder => seeder.SeedAsync(_mongoDatabaseMock.Object), Times.Once);
        }

        [Fact]
        public async Task InitializeAsync_Should_NotSeed_When_AlreadyInitialized()
        {
            // Arrange
            SetPrivateStaticField(typeof(MongoDbInitializer), "_initialized", 1);

            // Act
            await _mongoDbInitializer.InitializeAsync();

            // Assert
            _mongoDbSeederMock.Verify(seeder => seeder.SeedAsync(It.IsAny<IMongoDatabase>()), Times.Never);
        }

        private void SetPrivateStaticField(System.Type type, string fieldName, object value)
        {
            FieldInfo field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
            field?.SetValue(null, value);
        }
    }
}
