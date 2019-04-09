namespace Tinkoff.ISA.DAL.Elasticsearch.Request
{
    public class ConfluenceElasticSearchRequest : ElasticSearchRequest
    {
        public override string Index => Indexes.ConfluenceIndex;
    }
}
