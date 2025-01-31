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
            _context.Database.Migrate();
        }
    }
}
