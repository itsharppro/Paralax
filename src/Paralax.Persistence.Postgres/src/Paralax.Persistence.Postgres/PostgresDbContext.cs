using Microsoft.EntityFrameworkCore;

namespace Paralax.Persistence.Postgres
{
    public class PostgresDbContext : DbContext
    {
        public PostgresDbContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
