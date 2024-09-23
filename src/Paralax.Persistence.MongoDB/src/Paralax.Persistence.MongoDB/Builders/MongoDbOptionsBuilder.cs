namespace Paralax.Persistence.MongoDB.Builders
{
    internal sealed class MongoDbOptionsBuilder : IMongoDbOptionsBuilder
    {
        private readonly MongoDbOptions _options = new();

        public IMongoDbOptionsBuilder WithConnectionString(string connectionString)
        {
            _options.ConnectionString = connectionString;
            return this;
        }

        public IMongoDbOptionsBuilder WithDatabase(string database)
        {
            _options.Database = database;
            return this;
        }

        public IMongoDbOptionsBuilder WithSeed(bool seed)
        {
            _options.Seed = seed;
            return this;
        }

        public IMongoDbOptionsBuilder WithSetRandomDatabaseSuffix(bool setRandomDatabaseSuffix)
        {
            _options.SetRandomDatabaseSuffix = setRandomDatabaseSuffix;
            return this;
        }

        public IMongoDbOptionsBuilder WithUseSsl(bool useSsl)
        {
            _options.UseSsl = useSsl;
            return this;
        }

        public IMongoDbOptionsBuilder WithMaxConnectionPoolSize(int maxConnectionPoolSize)
        {
            _options.MaxConnectionPoolSize = maxConnectionPoolSize;
            return this;
        }

        public IMongoDbOptionsBuilder WithMinConnectionPoolSize(int minConnectionPoolSize)
        {
            _options.MinConnectionPoolSize = minConnectionPoolSize;
            return this;
        }

        public IMongoDbOptionsBuilder WithServerSelectionTimeoutMs(int timeoutMs)
        {
            _options.ServerSelectionTimeoutMs = timeoutMs;
            return this;
        }

        public IMongoDbOptionsBuilder WithConnectTimeoutMs(int timeoutMs)
        {
            _options.ConnectTimeoutMs = timeoutMs;
            return this;
        }

        public IMongoDbOptionsBuilder WithSocketTimeoutMs(int timeoutMs)
        {
            _options.SocketTimeoutMs = timeoutMs;
            return this;
        }

        public IMongoDbOptionsBuilder WithRetryWrites(bool retryWrites)
        {
            _options.RetryWrites = retryWrites;
            return this;
        }

        public IMongoDbOptionsBuilder WithRetryReads(bool retryReads)
        {
            _options.RetryReads = retryReads;
            return this;
        }

        public IMongoDbOptionsBuilder WithAuthentication(string username, string password, string authenticationMechanism)
        {
            _options.Username = username;
            _options.Password = password;
            _options.AuthenticationMechanism = authenticationMechanism;
            return this;
        }

        public IMongoDbOptionsBuilder WithUseCompression(bool useCompression)
        {
            _options.UseCompression = useCompression;
            return this;
        }

        public MongoDbOptions Build()
        {
            return _options;
        }
    }
}
