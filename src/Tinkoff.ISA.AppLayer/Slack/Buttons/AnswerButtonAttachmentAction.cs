using Tinkoff.ISA.DAL.Slack.Dtos;

namespace Tinkoff.ISA.AppLayer.Slack.Buttons
{
    public class AnswerButtonAttachmentAction : AttachmentActionDto
    {
        public const string ActionName = "answer";
        public AnswerButtonAttachmentAction(string valueJson) 
            : base(ActionName, "Answer")
        {
            Style = "primary";
            Type = "button";
            Value = valueJson;
        }
    }
}
