using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Nest;
using Tinkoff.ISA.DAL.Elasticsearch.Request;
using Tinkoff.ISA.Domain.Search;

namespace Tinkoff.ISA.DAL.Elasticsearch.Client
{
    public class ElasticsearchClient : IElasticsearchClient
    {
        private const string DefaultType = "doc";
        private readonly IElasticClientWrapper _elasticClient;

        public ElasticsearchClient(IElasticClientWrapper elasticClient)
        {
            _elasticClient = elasticClient;
        }

        public async Task<IList<TResponse>> SearchAsync<TResponse>(ElasticSearchRequest request,
            Func<QueryContainerDescriptor<TResponse>, QueryContainer> query) where TResponse : SearchableText
        {
            if (string.IsNullOrEmpty(request?.Index)) throw new ArgumentException(nameof(request.Index));
            
            var searchResponse = await _elasticClient.SearchAsync<TResponse>(s =>
                s.Index(request.Index)
                    .Type(DefaultType)
                    .From(request.Offset)
                    .Size(request.Count)
                    .Query(query));

            if (searchResponse.ApiCall.HttpStatusCode != (int)HttpStatusCode.OK)
            {
                throw new ElasticException(
                    $"{searchResponse.ApiCall.Uri} error: response status code {searchResponse.ApiCall.HttpStatusCode}");
            }

            return searchResponse.Documents?.ToList();
        }

        public Task UpsertManyAsync<TEntity>(ElasticUpsertRequest<TEntity> request)
            where TEntity : SearchableText
        {
            if (string.IsNullOrEmpty(request?.Index)) throw new ArgumentException(nameof(request.Index));

            return _elasticClient.BulkAsync(bd => bd.UpdateMany(
                    request.Entities,
                    (bud, a) => bud
                        .Id(a.Id)
                        .Index(request.Index)
                        .Type(DefaultType)
                        .DocAsUpsert()
                        .Doc(a)));
        }
    }
}
