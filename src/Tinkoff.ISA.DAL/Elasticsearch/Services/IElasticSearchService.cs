using System.Collections.Generic;
using System.Threading.Tasks;
using Tinkoff.ISA.DAL.Elasticsearch.Request;
using Tinkoff.ISA.Domain.Search;

namespace Tinkoff.ISA.DAL.Elasticsearch.Services
{
    public interface IElasticSearchService
    {
        Task<IList<TResponse>> SearchAsync<TResponse>(ElasticSearchRequest request) 
            where TResponse : SearchableText;

        Task<IList<TResponse>> SearchWithTitleAsync<TResponse>(ElasticSearchRequest request)
            where TResponse : SearchableWithTitle;
    }
}