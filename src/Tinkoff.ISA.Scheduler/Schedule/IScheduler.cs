using System;

namespace Tinkoff.ISA.Scheduler.Schedule
{
    public interface IScheduler : IDisposable
    {
        void StartJobs(object o);
    }
}
