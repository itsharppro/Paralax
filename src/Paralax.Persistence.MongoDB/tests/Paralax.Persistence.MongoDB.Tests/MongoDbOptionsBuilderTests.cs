using Xunit;
using Paralax.Persistence.MongoDB.Builders;

namespace Paralax.Persistence.MongoDB.Tests.Builders
{
    public class MongoDbOptionsBuilderTests
    {
        [Fact]
        public void WithConnectionString_Should_Set_ConnectionString()
        {
            // Arrange
            var builder = new MongoDbOptionsBuilder();
            var connectionString = "mongodb://localhost:27017";

            // Act
            var result = builder.WithConnectionString(connectionString).Build();

            // Assert
            Assert.Equal(connectionString, result.ConnectionString);
        }

        [Fact]
        public void WithDatabase_Should_Set_DatabaseName()
        {
            // Arrange
            var builder = new MongoDbOptionsBuilder();
            var databaseName = "TestDatabase";

            // Act
            var result = builder.WithDatabase(databaseName).Build();

            // Assert
            Assert.Equal(databaseName, result.Database);
        }

        [Fact]
        public void WithSeed_Should_Set_Seed()
        {
            // Arrange
            var builder = new MongoDbOptionsBuilder();

            // Act
            var result = builder.WithSeed(true).Build();

            // Assert
            Assert.True(result.Seed);
        }

        [Fact]
        public void WithSetRandomDatabaseSuffix_Should_Set_RandomDatabaseSuffix()
        {
            // Arrange
            var builder = new MongoDbOptionsBuilder();

            // Act
            var result = builder.WithSetRandomDatabaseSuffix(true).Build();

            // Assert
            Assert.True(result.SetRandomDatabaseSuffix);
        }

        [Fact]
        public void WithUseSsl_Should_Set_UseSsl()
        {
            // Arrange
            var builder = new MongoDbOptionsBuilder();

            // Act
            var result = builder.WithUseSsl(true).Build();

            // Assert
            Assert.True(result.UseSsl);
        }

        [Fact]
        public void WithMaxConnectionPoolSize_Should_Set_MaxConnectionPoolSize()
        {
            // Arrange
            var builder = new MongoDbOptionsBuilder();
            var maxPoolSize = 200;

            // Act
            var result = builder.WithMaxConnectionPoolSize(maxPoolSize).Build();

            // Assert
            Assert.Equal(maxPoolSize, result.MaxConnectionPoolSize);
        }

        [Fact]
        public void WithMinConnectionPoolSize_Should_Set_MinConnectionPoolSize()
        {
            // Arrange
            var builder = new MongoDbOptionsBuilder();
            var minPoolSize = 10;

            // Act
            var result = builder.WithMinConnectionPoolSize(minPoolSize).Build();

            // Assert
            Assert.Equal(minPoolSize, result.MinConnectionPoolSize);
        }

        [Fact]
        public void WithServerSelectionTimeoutMs_Should_Set_ServerSelectionTimeout()
        {
            // Arrange
            var builder = new MongoDbOptionsBuilder();
            var timeoutMs = 5000;

            // Act
            var result = builder.WithServerSelectionTimeoutMs(timeoutMs).Build();

            // Assert
            Assert.Equal(timeoutMs, result.ServerSelectionTimeoutMs);
        }

        [Fact]
        public void WithConnectTimeoutMs_Should_Set_ConnectTimeout()
        {
            // Arrange
            var builder = new MongoDbOptionsBuilder();
            var timeoutMs = 10000;

            // Act
            var result = builder.WithConnectTimeoutMs(timeoutMs).Build();

            // Assert
            Assert.Equal(timeoutMs, result.ConnectTimeoutMs);
        }

        [Fact]
        public void WithSocketTimeoutMs_Should_Set_SocketTimeout()
        {
            // Arrange
            var builder = new MongoDbOptionsBuilder();
            var timeoutMs = 60000;

            // Act
            var result = builder.WithSocketTimeoutMs(timeoutMs).Build();

            // Assert
            Assert.Equal(timeoutMs, result.SocketTimeoutMs);
        }

        [Fact]
        public void WithRetryWrites_Should_Set_RetryWrites()
        {
            // Arrange
            var builder = new MongoDbOptionsBuilder();

            // Act
            var result = builder.WithRetryWrites(false).Build();

            // Assert
            Assert.False(result.RetryWrites);
        }

        [Fact]
        public void WithRetryReads_Should_Set_RetryReads()
        {
            // Arrange
            var builder = new MongoDbOptionsBuilder();

            // Act
            var result = builder.WithRetryReads(false).Build();

            // Assert
            Assert.False(result.RetryReads);
        }

        [Fact]
        public void WithAuthentication_Should_Set_AuthenticationCredentials()
        {
            // Arrange
            var builder = new MongoDbOptionsBuilder();
            var username = "admin";
            var password = "password123";
            var authMechanism = "SCRAM-SHA-256";

            // Act
            var result = builder.WithAuthentication(username, password, authMechanism).Build();

            // Assert
            Assert.Equal(username, result.Username);
            Assert.Equal(password, result.Password);
            Assert.Equal(authMechanism, result.AuthenticationMechanism);
        }

        [Fact]
        public void WithUseCompression_Should_Set_UseCompression()
        {
            // Arrange
            var builder = new MongoDbOptionsBuilder();

            // Act
            var result = builder.WithUseCompression(true).Build();

            // Assert
            Assert.True(result.UseCompression);
        }
    }
}
