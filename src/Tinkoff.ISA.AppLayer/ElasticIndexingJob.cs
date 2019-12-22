using System;
using System.Linq;
using System.Threading.Tasks;
using Tinkoff.ISA.Core;
using Tinkoff.ISA.DAL.Elasticsearch.Client;
using Tinkoff.ISA.DAL.Elasticsearch.Request;

namespace Tinkoff.ISA.AppLayer
{
    // register jobs
    public class ElasticIndexingJob
    {
        private readonly IElasticSearchClient _elasticSearchClient;
        private readonly IKnowledgeProvider _knowledgeProvider;
        private readonly string _indexName;

        public ElasticIndexingJob(IElasticSearchClient elasticSearchClient,
            IKnowledgeProvider knowledgeProvider,
            string indexName)
        {
            _elasticSearchClient = elasticSearchClient;
            _knowledgeProvider = knowledgeProvider;
            _indexName = indexName;
        }

        public async Task Execute()
        {
            // provide storage for settings
            var knowledgeRequest = new KnowledgeRequest
            {
                StartAt = 5,
                StartDate = DateTimeOffset.UtcNow
            };
            
            var batch = await _knowledgeProvider.GetKnowledgeBatch(knowledgeRequest);

            var upsertRequest = new ElasticUpsertRequestV2(batch.Documents.ToList(), _indexName);
            
            await _elasticSearchClient.UpsertManyAsyncV2(upsertRequest);
        }
    }
}
