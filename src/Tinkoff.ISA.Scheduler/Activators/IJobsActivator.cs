using System.Threading.Tasks;

namespace Tinkoff.ISA.Scheduler.Activators
{
    public interface IJobsActivator
    {
        Task StartJobs();
    }
}
