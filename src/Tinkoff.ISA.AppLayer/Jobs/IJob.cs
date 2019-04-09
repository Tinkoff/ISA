using System.Threading.Tasks;

namespace Tinkoff.ISA.AppLayer.Jobs
{
    public interface IJob
    {
        Task StartJob();
    }
}
