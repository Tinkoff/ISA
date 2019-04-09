using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using Tinkoff.ISA.Domain;
using Tinkoff.ISA.Infrastructure.MongoDb;

namespace Tinkoff.ISA.DAL.Storage.Dao.Questions
{
    public class QuestionDao : IQuestionDao
    {
        private readonly IMongoCollection<Question> _collection;

        public QuestionDao(IMongoContext context)
        {
            _collection = context.For<Question>();
        }

        private static UpdateDefinition<Question> LastDateUpdateDefinition =>
            Builders<Question>.Update.Set(q => q.LastUpdate, DateTime.UtcNow);

        public Task<Question> UpsertAsync(Expression<Func<Question, bool>> filterExpr, Question question)
        {
            var filter = Builders<Question>.Filter.Where(filterExpr);
            
            var update = LastDateUpdateDefinition
                .SetOnInsert(q => q.Id, Guid.NewGuid())
                .AddToSetEach(q => q.AskedUsersIds, question.AskedUsersIds);
            
            var options = new FindOneAndUpdateOptions<Question>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };
            
            return _collection.FindOneAndUpdateAsync(filter, update, options);
        }

        public Task UpdateRankAsync(string questionId, string answerId, int value)
        {
            var filter = Builders<Question>.Filter.Where(q => q.Id == new Guid(questionId) && 
                                                              q.Answers.Any(a => a.Id == new Guid(answerId)));
            
            var update = LastDateUpdateDefinition.Inc(q => q.Answers.ElementAt(-1).Rank, value);
            
            return _collection.FindOneAndUpdateAsync(filter, update);
        }

        public Task PushNewItemAsync<TItem>(Expression<Func<Question, bool>> filterExpr,
            Expression<Func<Question, IEnumerable<TItem>>> updateExpr, TItem value)
        {
            var filter = Builders<Question>.Filter.Where(filterExpr);
            var update = LastDateUpdateDefinition.Push(updateExpr, value);
            
            return _collection.FindOneAndUpdateAsync(filter, update);
        }

        public Task RemoveOneItemAsync<TItem>(Expression<Func<Question, bool>> filterExpr,
            Expression<Func<Question, IEnumerable<TItem>>> removeExpr,
            TItem value)
        {
            var filter = Builders<Question>.Filter.Where(filterExpr);
            var update = Builders<Question>.Update.Pull(removeExpr, value);
            return _collection.FindOneAndUpdateAsync(filter, update);
        }


        public async Task<IList<TProjection>> FindWithProjectionAndFilterAsync<TProjection>(
            ProjectionDefinition<Question, TProjection> projection,
            FilterDefinition<Question> filter)
        {
            var options = new FindOptions<Question, TProjection> {Projection = projection};

            using (var cursor = await _collection.FindAsync(filter, options))
            {
                return await cursor.ToListAsync();
            }
        }
    }
}