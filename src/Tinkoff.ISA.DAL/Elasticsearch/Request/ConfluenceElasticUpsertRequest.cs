using Tinkoff.ISA.Domain.Search;

namespace Tinkoff.ISA.DAL.Elasticsearch.Request
{
    public class ConfluenceElasticUpsertRequest : ElasticUpsertRequest<SearchableConfluence>
    {
        public override string Index => Indexes.ConfluenceIndex;
    }
}
