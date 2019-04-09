namespace Tinkoff.ISA.DAL.Elasticsearch.Request
{
    public class JiraElasticSearchRequest : ElasticSearchRequest
    {
        public override string Index => Indexes.JiraIndex;
    }
}
