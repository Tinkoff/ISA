using Tinkoff.ISA.Domain.Search;

namespace Tinkoff.ISA.DAL.Elasticsearch.Request
{
    public class JiraElasticUpsertRequest : ElasticUpsertRequest<SearchableJira>
    {
        public override string Index => Indexes.JiraIndex;
    }
}
