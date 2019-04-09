using System.Threading.Tasks;

namespace Tinkoff.ISA.AppLayer.Slack.Routing
{
    public interface IRoutingService
    {
        Task Route(string payload);
    }
}
