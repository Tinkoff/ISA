using MongoDB.Driver;

namespace Tinkoff.ISA.Infrastructure.MongoDb
{
    public interface IMongoContext
    {
        IMongoCollection<TEntity> For<TEntity>();
    }
}
