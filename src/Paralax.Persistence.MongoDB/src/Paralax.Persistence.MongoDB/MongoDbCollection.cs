using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Paralax.Persistence.MongoDB
{
    public class MongoDbCollection<TEntity> : IMongoDbCollection<TEntity>
    {
        private readonly IMongoCollection<TEntity> _collection;

        public MongoDbCollection(IMongoDatabase database, string collectionName)
        {
            _collection = database.GetCollection<TEntity>(collectionName);
        }

        public Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> predicate)
            => _collection.Find(predicate).SingleOrDefaultAsync();

        public async Task<IReadOnlyList<TEntity>> ToListAsync(Expression<Func<TEntity, bool>> predicate)
            => await _collection.Find(predicate).ToListAsync();

        public Task AddAsync(TEntity entity)
            => _collection.InsertOneAsync(entity);

        public Task UpdateAsync(TEntity entity, Expression<Func<TEntity, bool>> predicate)
            => _collection.ReplaceOneAsync(predicate, entity);

        public Task DeleteAsync(Expression<Func<TEntity, bool>> predicate)
            => _collection.DeleteOneAsync(predicate);

        public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
            => _collection.Find(predicate).AnyAsync();

        public IMongoQueryable<TEntity> AsQueryable()
            => _collection.AsQueryable();

        public IMongoCollection<TEntity> AsCollection()
            => _collection; 
    }
}
