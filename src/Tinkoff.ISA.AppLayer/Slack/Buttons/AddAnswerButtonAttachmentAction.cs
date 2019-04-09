using Tinkoff.ISA.DAL.Slack.Dtos;

namespace Tinkoff.ISA.AppLayer.Slack.Buttons
{
    public class AddAnswerButtonAttachmentAction : AttachmentActionDto
    {
        public const string ActionName = "addAnswer";

        public AddAnswerButtonAttachmentAction(string valueJson)
            : base(ActionName, "Add answer")
        {
            Style = "primary";
            Type = "button";
            Value = valueJson;
        }
    }
}
