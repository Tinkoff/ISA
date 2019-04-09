namespace Tinkoff.ISA.DAL.Elasticsearch.Request
{
    public class AnswerElasticSearchRequest : ElasticSearchRequest
    {
        public override string Index => Indexes.AnswersIndex;
    }
}
