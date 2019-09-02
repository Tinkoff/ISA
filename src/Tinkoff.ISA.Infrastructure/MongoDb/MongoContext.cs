using System;
using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Tinkoff.ISA.Infrastructure.Settings;

namespace Tinkoff.ISA.Infrastructure.MongoDb
{
    public class MongoContext : IMongoContext
    {
        private readonly ConcurrentDictionary<Type, string> _collectionInfoCache;
        private readonly Lazy<MongoClient> _isaMongoClient;
        private readonly string _isaDbName;

        public IMongoClient MongoClient => _isaMongoClient.Value;
        
        static MongoContext()
        {
            BsonDefaults.GuidRepresentation = GuidRepresentation.Standard;

            ConventionRegistry.Register(
                "IgnoreExtraElements",
                new ConventionPack {new IgnoreExtraElementsConvention(true)},
                type => true
            );
        }

        public MongoContext(IOptions<ConnectionStringsSettings> connectionStrings)
        {
            _collectionInfoCache = new ConcurrentDictionary<Type, string>();

            var isaMongoUrl = new MongoUrl(connectionStrings.Value.MongoDb);
            _isaDbName = isaMongoUrl.DatabaseName;
            _isaMongoClient = new Lazy<MongoClient>(() => new MongoClient(isaMongoUrl));
        }

        private string GetCollectionName(Type entityType)
        {
            var collectionAttribute = (CollectionAttribute)entityType
                .GetTypeInfo()
                .GetCustomAttribute(typeof(CollectionAttribute));

            if (collectionAttribute == null)
            {
                throw new InvalidOperationException(
                    $"Entity '{entityType.GetTypeInfo().FullName}' " +
                    $"does not have a collection in MongoDB ({typeof(CollectionAttribute).Name} must be used)"
                );
            }

            if (string.IsNullOrWhiteSpace(collectionAttribute.CollectionName))
            {
                throw new InvalidOperationException(
                    $"An entity '{entityType.GetTypeInfo().FullName}' " +
                    $"has incorrect collection name ({typeof(CollectionAttribute).Name})"
                );
            }

            return collectionAttribute.CollectionName;
        }

        public IMongoCollection<TEntity> For<TEntity>()
        {
            var collectionName = _collectionInfoCache.GetOrAdd(typeof(TEntity), GetCollectionName);
            return _isaMongoClient.Value
                .GetDatabase(_isaDbName)
                .GetCollection<TEntity>(collectionName);
        }
    }
}
