using System.Collections.Generic;
using Newtonsoft.Json;

namespace Tinkoff.ISA.DAL.Confluence.Dtos
{
    public class ContentResponse
    {
        public IList<ContentDto> Results;

        [JsonProperty("_links")]
        public LinksDto Links { get; set; }
    }
}
