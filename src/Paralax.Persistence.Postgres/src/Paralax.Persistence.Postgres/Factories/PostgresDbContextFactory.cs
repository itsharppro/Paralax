using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Paralax.Persistence.Postgres.Factories
{
    public class PostgresDbContextFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public PostgresDbContextFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public PostgresDbContext Create()
        {
            return _serviceProvider.GetRequiredService<PostgresDbContext>();
        }
    }
}
