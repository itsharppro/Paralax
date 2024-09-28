using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Paralax.CQRS.Queries;
using Paralax.Types;

namespace Paralax.Persistence.MongoDB.Repositories
{
    public class MongoRepository<TEntity, TIdentifiable> : IMongoRepository<TEntity, TIdentifiable>
        where TEntity : class, IIdentifiable<TIdentifiable>
    {
        private readonly IMongoDbCollection<TEntity> _collection;

        public MongoRepository(IMongoDbCollection<TEntity> collection)
        {
            _collection = collection;
        }

        public IMongoCollection<TEntity> Collection => _collection.AsCollection();

        public Task<TEntity> GetAsync(TIdentifiable id)
            => _collection.FindAsync(e => e.Id.Equals(id));

        public Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate)
            => _collection.FindAsync(predicate);

        public async Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
            => await _collection.ToListAsync(predicate);

        public Task AddAsync(TEntity entity)
            => _collection.AddAsync(entity);

        public Task UpdateAsync(TEntity entity)
            => _collection.UpdateAsync(entity, e => e.Id.Equals(entity.Id));

        public Task UpdateAsync(TEntity entity, Expression<Func<TEntity, bool>> predicate)
            => _collection.UpdateAsync(entity, predicate);

        public Task DeleteAsync(TIdentifiable id)
            => _collection.DeleteAsync(e => e.Id.Equals(id));

        public Task DeleteAsync(Expression<Func<TEntity, bool>> predicate)
            => _collection.DeleteAsync(predicate);

        public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
            => _collection.ExistsAsync(predicate);

        public Task<PagedResult<TEntity>> BrowseAsync<TQuery>(Expression<Func<TEntity, bool>> predicate, TQuery query)
            where TQuery : IPagedQuery
        {
            return _collection.AsQueryable().Where(predicate).PaginateAsync(query);
        }
    }
}
