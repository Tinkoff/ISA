using System;
using Tinkoff.ISA.AppLayer.Slack.Common;
using Tinkoff.ISA.AppLayer.Slack.Dialogs.Submissions;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.Request;

namespace Tinkoff.ISA.AppLayer.Slack.TypeChooser.DialogSubmission
{
    internal class SubmissionSelectService : ISubmissionSelectService
    {
        public Type Choose(string callbackId)
        {
            if (callbackId.StartsWith(CallbackId.AddAnswerDialogSubmissionId))
                return typeof(DialogSubmission<AddAnswerSubmission>);
            throw new ArgumentOutOfRangeException();
        }
    }
}
