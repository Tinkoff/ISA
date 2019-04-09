using Tinkoff.ISA.Domain.Search;

namespace Tinkoff.ISA.DAL.Elasticsearch.Request
{
    public class QuestionsElasticUpsertRequest : ElasticUpsertRequest<SearchableQuestion>
    {
        public override string Index => Indexes.QuestionsIndex;
    }
}