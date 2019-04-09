using System.Threading.Tasks;

namespace Tinkoff.ISA.AppLayer.Slack
{   
    public interface ISlackActionHandler<in TParams> where TParams : ISlackActionParams
    {
        Task Handle(TParams actionParams);
    }
}
