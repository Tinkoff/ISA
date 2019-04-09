using System.Collections.Generic;

namespace Tinkoff.ISA.Infrastructure.Settings
{
    public class ElasticsearchSettings
    {
        public string Url { get; set; }

        public Dictionary<string, IndexSearchParams> IndexSearchParams { get; set; }
    }
}
