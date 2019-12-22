using MongoDB.Driver;

namespace Tinkoff.ISA.Infrastructure.MongoDb
{
    public interface IMongoContext
    {
        IMongoClient MongoClient { get; }
        IMongoCollection<TEntity> For<TEntity>();
    }
}
