using System;
using System.Threading.Tasks;
using Nest;

namespace Tinkoff.ISA.DAL.Elasticsearch.Client
{
    public interface IElasticClientWrapper
    {
        Task<ISearchResponse<T>> SearchAsync<T>(Func<SearchDescriptor<T>, ISearchRequest> func)
            where T : class;

        Task BulkAsync(Func<BulkDescriptor, IBulkRequest> selector);
    }
}
