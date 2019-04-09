using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace Tinkoff.ISA.AppLayer.Slack.Event.Request
{
    public class SlackEvent
    {
        public string Type { get; set; }

        [JsonProperty("bot_id")]
        public string BotId { get; set; }

        [JsonProperty("user")]
        public string UserId { get; set; }

        [BindRequired]
        public string Text { get; set; }

        [BindRequired]
        public string Channel { get; set; }
    }
}
