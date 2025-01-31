namespace Paralax.Persistence.Postgres.Builders
{
    public class PostgresOptionsBuilder
    {
        private readonly PostgresDbOptions _options = new();

        public PostgresOptionsBuilder WithConnectionString(string connectionString)
        {
            _options.ConnectionString = connectionString;
            return this;
        }

        public PostgresOptionsBuilder WithSeed(bool seed)
        {
            _options.Seed = seed;
            return this;
        }

        public PostgresOptionsBuilder EnableLogging(bool enableLogging)
        {
            _options.EnableLogging = enableLogging;
            return this;
        }

        public PostgresDbOptions Build() => _options;
    }
}
