using Tinkoff.ISA.DAL.Slack.Dtos;

namespace Tinkoff.ISA.AppLayer.Slack.Buttons
{
    public class NotHelpedButtonAttachmentAction : AttachmentActionDto
    {
        public const string ActionName = "notHelped";

        public NotHelpedButtonAttachmentAction(string valueJson) 
            : base(ActionName, "Useless")
        {
            Type = "button";
            Style = "danger";
            Value = valueJson;
        }
    }
}
