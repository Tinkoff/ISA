namespace Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.Request
{
    public class DialogSubmission<TSubmission> : InvocationPayloadRequest
    {
        public TSubmission Submission { get; set; }
    }
}
