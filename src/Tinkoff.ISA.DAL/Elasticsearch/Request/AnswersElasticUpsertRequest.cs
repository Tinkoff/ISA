using Tinkoff.ISA.Domain.Search;

namespace Tinkoff.ISA.DAL.Elasticsearch.Request
{
    public class AnswersElasticUpsertRequest : ElasticUpsertRequest<SearchableAnswer>
    {
        public override string Index => Indexes.AnswersIndex;
    }
}