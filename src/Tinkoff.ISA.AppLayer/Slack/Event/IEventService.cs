using System.Threading.Tasks;
using Tinkoff.ISA.AppLayer.Slack.Event.Request;

namespace Tinkoff.ISA.AppLayer.Slack.Event
{
    public interface IEventService
    { 
        Task ProcessRequest(EventWrapperRequest request);
    }
}
