using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Tinkoff.ISA.DAL.Storage.Dao.Questions;
using Tinkoff.ISA.Domain;
using Tinkoff.ISA.Domain.Search;

namespace Tinkoff.ISA.AppLayer.Search
{
    internal class SearchableTextService : ISearchableTextService
    {
        private readonly IQuestionDao _questionDao;

        public SearchableTextService(IQuestionDao questionDao)
        {
            _questionDao = questionDao;
        }

        public Task<IList<SearchableQuestion>> GetQuestionsAsync(IEnumerable<Guid> questionsIds)
        {
            if (questionsIds == null) throw new ArgumentNullException(nameof(questionsIds));

            var projection = Builders<Question>.Projection.Expression(
                q => new SearchableQuestion
                {
                    Id = q.Id.ToString(),
                    Text = q.Text
                });
            
            var filter = Builders<Question>.Filter.In(q => q.Id, questionsIds);
            
            return _questionDao.FindWithProjectionAndFilterAsync(projection, filter);
        }

        public Task<IList<SearchableQuestion>> GetQuestionsWithAnswersAsync(DateTime startDate)
        {
            var projection = Builders<Question>.Projection.Expression(
                q => new SearchableQuestion
                {
                    Id = q.Id.ToString(),
                    Text = q.Text
                });
            
            var filter = Builders<Question>.Filter.Gte(q => q.LastUpdate, startDate);
            filter &= Builders<Question>.Filter.SizeGt(q => q.Answers, 0);
            
            return _questionDao.FindWithProjectionAndFilterAsync(projection, filter);
        }

        public async Task<IList<SearchableAnswer>> GetAnswersAsync(DateTime startDate)
        {
            var projection = Builders<Question>.Projection.Include(q => q.Answers);
            var questions = await _questionDao.FindWithProjectionAndFilterAsync<Question>(
                projection, FilterDefinition<Question>.Empty);

            var answers = questions
                .Where(q => q.Answers != null)
                .SelectMany(q => q.Answers
                    .Where(a => a.LastUpdate.CompareTo(startDate) > 0)
                    .Select(a => new SearchableAnswer
                    {
                        Id = a.Id.ToString(),
                        QuestionId = q.Id.ToString(),
                        Text = a.Text
                    })
                ).ToList();

            return answers;
        }
    }
}
