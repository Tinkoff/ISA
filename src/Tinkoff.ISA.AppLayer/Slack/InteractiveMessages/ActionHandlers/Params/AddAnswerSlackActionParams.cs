using Tinkoff.ISA.AppLayer.Slack.Event.ButtonParams;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.Request;
using Tinkoff.ISA.DAL.Slack.Dtos;

namespace Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.ActionHandlers.Params
{
    public class AddAnswerSlackActionParams : ISlackActionParams
    {
        public string TriggerId { get; set; }

        public AddAnswerActionButtonParams ButtonParams { get; set; }

        public ItemInfo User { get; set; }

        public OriginalMessageDto OriginalMessage { get; set; }
    }
}
