using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Tinkoff.ISA.AppLayer.Slack.Event.Request
{
    public class EventWrapperRequest
    {
        
        public string Token { get; set; }

        public string TeamId { get; set; }

        public string Challenge { get; set; }

        public string Type { get; set; }

        [BindRequired]
        public SlackEvent Event { get; set; }
    }
}
