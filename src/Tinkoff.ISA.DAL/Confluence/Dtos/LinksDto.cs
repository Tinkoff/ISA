using Newtonsoft.Json;

namespace Tinkoff.ISA.DAL.Confluence.Dtos
{
    [JsonObject(Title = "_links")]
    public class LinksDto
    {
        public string Next { get; set; }

        public string Webui { get; set; }
    }
}
