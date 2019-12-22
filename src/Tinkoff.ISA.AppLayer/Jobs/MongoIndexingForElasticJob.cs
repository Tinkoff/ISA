using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tinkoff.ISA.AppLayer.Search;
using Tinkoff.ISA.DAL.Elasticsearch.Client;
using Tinkoff.ISA.DAL.Elasticsearch.Request;
using Tinkoff.ISA.DAL.Storage.Dao.Application;

namespace Tinkoff.ISA.AppLayer.Jobs
{
    public class MongoIndexingForElasticJob : IJob
    {
        private readonly ISearchableTextService _searchableTextService;
        private readonly IElasticSearchClient _elasticSearchClient;
        private readonly IApplicationPropertyDao _applicationPropertyDao;
        private readonly ILogger<MongoIndexingForElasticJob> _logger;

        public MongoIndexingForElasticJob(
            ISearchableTextService searchableTextService,
            IElasticSearchClient elasticSearchClient,
            IApplicationPropertyDao applicationPropertyDao,
            ILogger<MongoIndexingForElasticJob> logger)
        {
            _searchableTextService = searchableTextService;
            _elasticSearchClient = elasticSearchClient;
            _applicationPropertyDao = applicationPropertyDao;
            _logger = logger;
        }
        
        public async Task StartJob()
        {
            var appProperties = await _applicationPropertyDao.GetAsync();
            var startDate = appProperties?.LastMongoIndexing ?? DateTime.MinValue;

            await IndexAnswers(startDate);
            await IndexQuestions(startDate);
            
            await _applicationPropertyDao.UpsertPropertyAsync(app => app.LastMongoIndexing, DateTime.UtcNow);
        }

        private async Task IndexAnswers(DateTime startDate)
        {
            var answers = await _searchableTextService.GetAnswersAsync(startDate);
            
            if (answers.Count == 0)
            {
                _logger.LogInformation("{JobName} | There are no new answers to index", nameof(MongoIndexingForElasticJob));
                return;
            }
            
            var request = new AnswersElasticUpsertRequest
            {
                Entities = answers.ToList()
            };
                
            await _elasticSearchClient.UpsertManyAsync(request);
        }

        private async Task IndexQuestions(DateTime startDate)
        {
            var questions = await _searchableTextService.GetQuestionsWithAnswersAsync(startDate);
            
            if (questions.Count == 0)
            {
                _logger.LogInformation("{JobName} | There are no new questions to index", nameof(MongoIndexingForElasticJob));
                return;
            }
            
            var request = new QuestionsElasticUpsertRequest
            {
                Entities = questions.ToList()
            };
                
            await _elasticSearchClient.UpsertManyAsync(request);
        }
    }
}