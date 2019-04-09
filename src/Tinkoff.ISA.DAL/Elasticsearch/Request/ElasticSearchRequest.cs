using Tinkoff.ISA.Infrastructure.Settings;

namespace Tinkoff.ISA.DAL.Elasticsearch.Request
{
    public abstract class ElasticSearchRequest : ElasticRequest
    {
        public string Text { get; set; }

        public int Offset { get; set; }

        public int Count { get; set; }

        public IndexSearchParams SearchParams { get; set; }
    }
}
