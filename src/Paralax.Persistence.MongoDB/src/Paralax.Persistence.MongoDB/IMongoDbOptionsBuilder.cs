namespace Paralax.Persistence.MongoDB.Builders
{
    public interface IMongoDbOptionsBuilder
    {
        IMongoDbOptionsBuilder WithConnectionString(string connectionString);
        IMongoDbOptionsBuilder WithDatabase(string database);
        IMongoDbOptionsBuilder WithSeed(bool seed);
        IMongoDbOptionsBuilder WithSetRandomDatabaseSuffix(bool setRandomDatabaseSuffix);
        IMongoDbOptionsBuilder WithUseSsl(bool useSsl);
        IMongoDbOptionsBuilder WithMaxConnectionPoolSize(int maxConnectionPoolSize);
        IMongoDbOptionsBuilder WithMinConnectionPoolSize(int minConnectionPoolSize);
        IMongoDbOptionsBuilder WithServerSelectionTimeoutMs(int timeoutMs);
        IMongoDbOptionsBuilder WithConnectTimeoutMs(int timeoutMs);
        IMongoDbOptionsBuilder WithSocketTimeoutMs(int timeoutMs);
        IMongoDbOptionsBuilder WithRetryWrites(bool retryWrites);
        IMongoDbOptionsBuilder WithRetryReads(bool retryReads);
        IMongoDbOptionsBuilder WithAuthentication(string username, string password, string authenticationMechanism);
        IMongoDbOptionsBuilder WithUseCompression(bool useCompression);

        MongoDbOptions Build();
    }
}
