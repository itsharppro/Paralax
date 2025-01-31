using System.ComponentModel;

namespace Paralax.Persistence.Postgres
{
    public class PostgresDbOptions
    {
        public string ConnectionString { get; set; }
        public bool Seed { get; set; } = false;
        public bool EnableLogging { get; set; } = false;
    }
}
