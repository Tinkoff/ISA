using Tinkoff.ISA.DAL.Slack.Dtos;

namespace Tinkoff.ISA.AppLayer.Slack.Buttons
{
    public class AskExpertsButtonAttachmentAction : AttachmentActionDto
    {
        public const string ActionName = "askExperts";

        public AskExpertsButtonAttachmentAction(string questionText) 
            : base(ActionName, "Ask experts")
        {
            Type = "button";
            Value = questionText;
        }
    }
}
