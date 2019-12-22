using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tinkoff.ISA.DAL.Elasticsearch.Request;
using Tinkoff.ISA.Domain.Search;
using Nest;

namespace Tinkoff.ISA.DAL.Elasticsearch.Client
{
    public interface IElasticSearchClient
    {
        Task<IList<TResponse>> SearchAsync<TResponse>(ElasticSearchRequest request, 
            Func<QueryContainerDescriptor<TResponse>, QueryContainer> query) where TResponse : SearchableText;

        Task UpsertManyAsync<TEntity>(ElasticUpsertRequest<TEntity> requests)
            where TEntity : SearchableText;
        
        Task UpsertManyAsyncV2(ElasticUpsertRequestV2 request);
    }
}
