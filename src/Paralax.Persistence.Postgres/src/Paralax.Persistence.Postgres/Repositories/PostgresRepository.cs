using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Paralax.Types;

namespace Paralax.Persistence.Postgres.Repositories
{
    public class PostgresRepository<TEntity, TIdentifiable> : IPostgresRepository<TEntity, TIdentifiable>
        where TEntity : class, IIdentifiable<TIdentifiable>
    {
        private readonly PostgresDbContext _context;
        private readonly DbSet<TEntity> _dbSet;

        public PostgresRepository(PostgresDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }

        public async Task<TEntity> GetAsync(TIdentifiable id)
            => await _dbSet.FindAsync(id);

        public async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate)
            => await _dbSet.FirstOrDefaultAsync(predicate);

        public async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
            => await _dbSet.Where(predicate).ToListAsync();

        public async Task AddAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TEntity entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(TIdentifiable id)
        {
            var entity = await GetAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
            => await _dbSet.AnyAsync(predicate);
    }
}
