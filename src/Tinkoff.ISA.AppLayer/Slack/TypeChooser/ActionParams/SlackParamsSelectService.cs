using System;
using Tinkoff.ISA.AppLayer.Slack.Buttons;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.ActionHandlers.Params;

namespace Tinkoff.ISA.AppLayer.Slack.TypeChooser.ActionParams
{
    internal class SlackParamsSelectService : ISlackParamsSelectService
    {
        public Type Choose(string actionName)
        {
            switch (actionName)
            {
                case HelpedButtonAttachmentAction.ActionName:
                    return typeof(HelpedSlackActionParams);
                case NotHelpedButtonAttachmentAction.ActionName:
                    return typeof(NotHelpedSlackActionParams);
                case AskExpertsButtonAttachmentAction.ActionName:
                    return typeof(AskExpertsSlackActionParams);
                case ShowMoreAnswersButtonAttachmentAction.ActionName:
                    return typeof(ShowMoreAnswersSlackActionParams);
                case AnswerButtonAttachmentAction.ActionName:
                    return typeof(AnswerSlackActionParams);
                case AddAnswerButtonAttachmentAction.ActionName:
                    return typeof(AddAnswerSlackActionParams);
                case ShowAnswersButtonAttachmentAction.ActionName:
                    return typeof(ShowAnswersSlackActionParams);
                case StopNotificationsButtonAttachmentAction.ActionName:
                    return typeof(StopNotificationsSlackActionParams);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
