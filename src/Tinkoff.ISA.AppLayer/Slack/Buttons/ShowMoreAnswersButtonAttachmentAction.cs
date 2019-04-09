using Tinkoff.ISA.DAL.Slack.Dtos;

namespace Tinkoff.ISA.AppLayer.Slack.Buttons
{
    public class ShowMoreAnswersButtonAttachmentAction : AttachmentActionDto
    {
        public const string ActionName = "more";

        public ShowMoreAnswersButtonAttachmentAction(string valueJson) 
            : base(ActionName, "Other answers")
        {
            Type = "button";
            Value = valueJson;
        }
    }
}
