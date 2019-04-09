using System;

namespace Tinkoff.ISA.AppLayer.Slack.TypeChooser.DialogSubmission
{
    public interface ISubmissionSelectService
    {
        Type Choose(string callbackId);
    }
}
