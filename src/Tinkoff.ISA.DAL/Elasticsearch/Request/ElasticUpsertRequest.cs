using System.Collections.Generic;
using Tinkoff.ISA.Domain.Search;

namespace Tinkoff.ISA.DAL.Elasticsearch.Request
{
    public abstract class ElasticUpsertRequest<TEntity> : ElasticRequest
        where TEntity : SearchableText
    {
        public List<TEntity> Entities { get; set; }
    }
}
