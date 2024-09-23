using System.Linq.Expressions;
using MongoDB.Driver.Linq;

namespace Paralax.Persistence.MongoDB
{
    public interface IMongoDbCollection<TEntity>
    {
        Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> predicate);
        Task<IReadOnlyList<TEntity>> ToListAsync(Expression<Func<TEntity, bool>> predicate);
        Task AddAsync(TEntity entity);
        Task UpdateAsync(TEntity entity, Expression<Func<TEntity, bool>> predicate);
        Task DeleteAsync(Expression<Func<TEntity, bool>> predicate);
        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);
        IMongoQueryable<TEntity> AsQueryable();
    }
}
