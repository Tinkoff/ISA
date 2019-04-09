using Tinkoff.ISA.DAL.Slack.Dtos;

namespace Tinkoff.ISA.AppLayer.Slack.Buttons
{
    public class HelpedButtonAttachmentAction : AttachmentActionDto
    {
        public const string ActionName = "helped";

        public HelpedButtonAttachmentAction(string valueJson) 
            : base(ActionName, "Helped me")
        {
            Type = "button";
            Style = "primary";
            Value = valueJson;
        }
    }
}