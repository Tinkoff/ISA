namespace Tinkoff.ISA.Infrastructure.Settings
{
    // https://www.elastic.co/guide/en/elasticsearch/reference/current/query-dsl-fuzzy-query.html
    // https://www.elastic.co/guide/en/elasticsearch/reference/current/query-dsl-minimum-should-match.html
    // https://www.elastic.co/guide/en/elasticsearch/reference/current/query-dsl-multi-match-query.html#type-best-fields
    public class IndexSearchParams
    {
        public double MinShouldMatchPercentage { get; set; }

        public int PrefixLength { get; set; }

        public int MaxExpansions { get; set; }
    }
}
