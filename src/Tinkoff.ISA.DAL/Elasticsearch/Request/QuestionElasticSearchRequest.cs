namespace Tinkoff.ISA.DAL.Elasticsearch.Request
{
    public class QuestionElasticSearchRequest : ElasticSearchRequest
    {
        public override string Index => Indexes.QuestionsIndex;
    }
}