using Newtonsoft.Json;

namespace Tinkoff.ISA.DAL.Confluence.Dtos
{
    public class ContentDto
    {
        public string Id { get; set; } 

        public string Title { get; set; }

        public ContentBodyDto Body { get; set; }

        public VersionDto Version { get; set; }

        [JsonProperty("_links")]
        public LinksDto Links { get; set; }
    }
}
