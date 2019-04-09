using Tinkoff.ISA.AppLayer.Slack.Event.ButtonParams;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.Request;
using Tinkoff.ISA.DAL.Slack.Dtos;

namespace Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.ActionHandlers.Params
{
    public class NotHelpedSlackActionParams : ISlackActionParams
    {
        public NotHelpedAnswerActionButtonParams ButtonParams { get; set; }

        public ItemInfo Channel { get; set; }

        public OriginalMessageDto OriginalMessage { get; set; }

        public int AttachmentId { get; set; }

        public ItemInfo User { get; set; }
    }
}
