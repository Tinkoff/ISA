using Newtonsoft.Json;

namespace Tinkoff.ISA.DAL.Slack.Dtos
{
    public class FieldDto
    {
        public string Title { get; set; }

        public string Value { get; set; }

        [JsonProperty("@short")]
        public bool Short { get; set; }
    }
}
