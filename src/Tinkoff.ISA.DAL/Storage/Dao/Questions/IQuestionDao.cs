using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using Tinkoff.ISA.Domain;

namespace Tinkoff.ISA.DAL.Storage.Dao.Questions
{
    public interface IQuestionDao
    {
        Task<Question> UpsertAsync(Expression<Func<Question, bool>> filterExpr, Question upsertValue);

        Task UpdateRankAsync(string questionId, string answerId, int value);

        Task PushNewItemAsync<TItem>(Expression<Func<Question, bool>> filterExpr,
            Expression<Func<Question, IEnumerable<TItem>>> updateExpr,
            TItem value);
        
        Task RemoveOneItemAsync<TItem>(Expression<Func<Question, bool>> filterExpr,
            Expression<Func<Question, IEnumerable<TItem>>> updateExpr,
            TItem value);

        Task<IList<TProjection>> FindWithProjectionAndFilterAsync<TProjection>(
            ProjectionDefinition<Question, TProjection> projection,
            FilterDefinition<Question> filter);
    }
}
