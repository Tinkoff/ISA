using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using Tinkoff.ISA.Domain.Application;
using Tinkoff.ISA.Infrastructure.MongoDb;

namespace Tinkoff.ISA.DAL.Storage.Dao.Application
{
    public class ApplicationPropertyDao : IApplicationPropertyDao
    {
        private readonly IMongoCollection<ApplicationProperty> _collection;

        public ApplicationPropertyDao(IMongoContext context)
        {
            _collection = context.For<ApplicationProperty>();
        }

        public Task UpsertPropertyAsync<TProperty>(Expression<Func<ApplicationProperty, TProperty>> updateExpr, TProperty value)
        {
            if (updateExpr == null) throw new ArgumentNullException(nameof(updateExpr));
            if (value == null) throw new ArgumentNullException(nameof(value));

            var filter = Builders<ApplicationProperty>.Filter.Empty;
            var update = Builders<ApplicationProperty>.Update.Set(updateExpr, value);
            var options = new UpdateOptions { IsUpsert = true };

            return _collection.UpdateOneAsync(filter, update, options);
        }

        public Task<ApplicationProperty> GetAsync()
        {
            var filter = Builders<ApplicationProperty>.Filter.Empty;
            return _collection.Find(filter).SingleOrDefaultAsync();
        }
    }
}
