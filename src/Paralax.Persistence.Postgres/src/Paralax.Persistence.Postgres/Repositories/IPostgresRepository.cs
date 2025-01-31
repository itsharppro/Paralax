using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Paralax.Types;

namespace Paralax.Persistence.Postgres.Repositories
{
    public interface IPostgresRepository<TEntity, in TIdentifiable> where TEntity : class, IIdentifiable<TIdentifiable>
    {
        Task<TEntity> GetAsync(TIdentifiable id);
        Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate);
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
        Task AddAsync(TEntity entity);
        Task UpdateAsync(TEntity entity);
        Task DeleteAsync(TIdentifiable id);
        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);
    }
}
