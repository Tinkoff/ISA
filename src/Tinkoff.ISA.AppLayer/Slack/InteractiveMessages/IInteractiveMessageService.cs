using System.Threading.Tasks;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.Request;

namespace Tinkoff.ISA.AppLayer.Slack.InteractiveMessages
{
    public interface IInteractiveMessageService
    {
        Task ProcessRequest(InteractiveMessage request);
    }
}
