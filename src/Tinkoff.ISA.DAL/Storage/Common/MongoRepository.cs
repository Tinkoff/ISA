using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Tinkoff.ISA.Infrastructure.Exceptions;
using Tinkoff.ISA.Infrastructure.MongoDb;
using Tinkoff.ISA.Infrastructure.Utils;

namespace Tinkoff.ISA.DAL.Storage.Common
{
    public class MongoRepository<TEntity, TKey> : IRepository<TEntity, TKey>
        where TEntity : class
    {
        private readonly Type _keyType;
        private readonly PropertyInfo _keyProperty;
        private readonly IMongoCollection<TEntity> _collection;

        public MongoRepository(IMongoContext context)
        {
            var mongoContext = context ?? throw new ArgumentNullException(nameof(context));
            _collection = mongoContext.For<TEntity>() ?? throw new ArgumentNullException(nameof(_collection));

            var entityType = typeof(TEntity);
            _keyType = typeof(TKey);

            _keyProperty = entityType
                .GetTypeInfo()
                .GetProperties()    
                .SingleOrDefault(p => p.HasAttribute<BsonIdAttribute>());

            if (_keyProperty == null)
            {
                throw new InvalidOperationException($"Entity {entityType.FullName} should have one ID");
            }
            if (_keyProperty.PropertyType != _keyType)
            {
                throw new InvalidTypeParameterException(entityType, _keyProperty.PropertyType, _keyType);
            }
        }

        public async Task<ICollection<TEntity>> Find(Expression<Func<TEntity, bool>> filterExpr)
        {
            if (filterExpr == null)
                throw new ArgumentException(nameof(filterExpr));
            
            var filter = Builders<TEntity>.Filter.Where(filterExpr);
            using (var cursor = await _collection.FindAsync(filter))
            {
                return await cursor.ToListAsync();
            }
        }

        public async Task<TEntity> FirstOrDefault(Expression<Func<TEntity, bool>> filterExpr)
        {
            if (filterExpr == null)
            {
                throw new ArgumentException(nameof(filterExpr));
            }

            var filter = Builders<TEntity>.Filter.Where(filterExpr);
            var findOptions = new FindOptions<TEntity, TEntity> { Limit = 1 };
            using (var cursor = await _collection.FindAsync(filter, findOptions))
            {
                await cursor.MoveNextAsync();
                return cursor.Current.FirstOrDefault();
            }
        }

        public async Task<TEntity> Get(TKey id)
        {
            if (id == null || id.Equals(default(TKey)))
            {
                throw new ArgumentException(nameof(id));
            }
            var filterExpr = GetFilterByKey(id);

            var filter = Builders<TEntity>.Filter.Where(filterExpr);
            var findOptions = new FindOptions<TEntity, TEntity> { Limit = 1 };
            using (var cursor = await _collection.FindAsync(filter, findOptions))
            {
                await cursor.MoveNextAsync();
                return cursor.Current.FirstOrDefault();
            }
        }

        public Task Add(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentException(nameof(entity));

            return _collection.InsertOneAsync(entity);
        }

        public async Task<long> Count()
        {
            return await _collection.CountDocumentsAsync(FilterDefinition<TEntity>.Empty);
        }

        private Expression<Func<TEntity, bool>> GetFilterByKey(TKey key)
        {
            var parameter = Expression.Parameter(typeof(TEntity), "x");
            var property = Expression.Property(parameter, _keyProperty.Name);
            var keyValue = Expression.Constant(key, _keyType);
            var equalsMethod = property.Type.GetMethod("Equals", new[] { keyValue.Type });
            var equalsMethodCall = Expression.Call(property, equalsMethod, keyValue);
            
            return Expression.Lambda<Func<TEntity, bool>>(equalsMethodCall, parameter);
        }
    }
}
