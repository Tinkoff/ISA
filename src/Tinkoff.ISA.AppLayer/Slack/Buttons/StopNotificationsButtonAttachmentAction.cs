using Tinkoff.ISA.DAL.Slack.Dtos;

namespace Tinkoff.ISA.AppLayer.Slack.Buttons
{
    public class StopNotificationsButtonAttachmentAction : AttachmentActionDto
    {
        public const string ActionName = "StopNotifications";

        public StopNotificationsButtonAttachmentAction(string valueJson)
            : base(ActionName, "Unsubscribe from notifications")
        {
            Type = "button";
            Value = valueJson;
            Style = "primary";
        }
    }
}