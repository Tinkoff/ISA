using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Nest;
using Tinkoff.ISA.Infrastructure.Settings;

namespace Tinkoff.ISA.DAL.Elasticsearch.Client
{
    public class ElasticClientWrapper : IElasticClientWrapper
    {
        private readonly IElasticClient _elasticClient;

        public ElasticClientWrapper(IOptions<ElasticSearchSettings> elasticSearchSettings)
        {
            _elasticClient = new ElasticClient(new Uri(elasticSearchSettings.Value.Url));
        }

        public Task<ISearchResponse<T>> SearchAsync<T>(Func<SearchDescriptor<T>, ISearchRequest> func)
            where T : class => _elasticClient.SearchAsync(func);

        public Task BulkAsync(Func<BulkDescriptor, IBulkRequest> selector) => _elasticClient.BulkAsync(selector);
    }
}
