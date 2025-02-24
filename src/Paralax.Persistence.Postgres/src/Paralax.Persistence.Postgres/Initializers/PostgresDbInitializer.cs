using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Paralax.Persistence.Postgres.Initializers
{
    public class PostgresDbInitializer : IPostgresDbInitializer
    {
        private readonly PostgresDbContext _context;

        public PostgresDbInitializer(PostgresDbContext context)
        {
            _context = context;
        }

        public async Task InitializeAsync()
        {
             var availableMigrations = _context.Database.GetMigrations();
            if (!availableMigrations.Any())
            {
                // No migrations are defined – create the schema directly.
                await _context.Database.EnsureCreatedAsync();
            }
            else
            {
                // Migrations exist – apply any pending migrations.
                await _context.Database.MigrateAsync();
            }
        }
    }
}
