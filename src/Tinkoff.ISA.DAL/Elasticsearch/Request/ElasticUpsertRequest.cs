using System.Collections.Generic;
using Tinkoff.ISA.Core.Documents;
using Tinkoff.ISA.Domain.Search;

namespace Tinkoff.ISA.DAL.Elasticsearch.Request
{
    public abstract class ElasticUpsertRequest<TEntity> : ElasticRequest
        where TEntity : SearchableText
    {
        public List<TEntity> Entities { get; set; }
    }
    
    public class ElasticUpsertRequestV2 : ElasticRequest
    {
        public ElasticUpsertRequestV2(List<ISearchableDocument> entities, string index)
        {
            Entities = entities;
            Index = index;
        }
        
        public List<ISearchableDocument> Entities { get; }
        
        public override string Index { get; }
    }
}
