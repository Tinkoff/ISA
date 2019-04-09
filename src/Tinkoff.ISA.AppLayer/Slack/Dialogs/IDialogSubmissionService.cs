using System.Threading.Tasks;

namespace Tinkoff.ISA.AppLayer.Slack.Dialogs
{
    public interface IDialogSubmissionService<in TSubmission>
    {
        Task ProcessSubmission(TSubmission submission);
    }
}
