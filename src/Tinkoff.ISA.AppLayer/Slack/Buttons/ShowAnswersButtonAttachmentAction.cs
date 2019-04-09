using Tinkoff.ISA.DAL.Slack.Dtos;

namespace Tinkoff.ISA.AppLayer.Slack.Buttons
{
    class ShowAnswersButtonAttachmentAction : AttachmentActionDto
    {
        public const string ActionName = "ShowAnswers";

        public ShowAnswersButtonAttachmentAction(string valueJson)
            : base(ActionName, "See answers")
        {
            Type = "button";
            Value = valueJson;
            Style = "primary";
        }
    }
}