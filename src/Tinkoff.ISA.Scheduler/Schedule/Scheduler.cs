using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tinkoff.ISA.Infrastructure.Settings;
using Tinkoff.ISA.Scheduler.Activators;

namespace Tinkoff.ISA.Scheduler.Schedule
{
    internal class Scheduler : IScheduler
    {
        private readonly Timer _timer;
        private readonly ILogger<Scheduler> _logger;
        private readonly IJobsActivator _jobsActivator;
 
        public Scheduler(
            ILogger<Scheduler> logger, 
            IOptions<SchedulerSettings> schedulerSettings, 
            IJobsActivator jobsActivator)
        {
            _logger = logger;
            _jobsActivator = jobsActivator;
            
            var period = TimeSpan.FromMinutes(schedulerSettings.Value.LoopTimeMinutes);
            _timer = new Timer(StartJobs, null, TimeSpan.Zero, period);
        }

        public void StartJobs(object o)
        {
            try
            {
                _logger.LogInformation("{JobsLaunchDate} Jobs started", DateTime.Now);
                _jobsActivator.StartJobs();
                _logger.LogInformation("{JobsFinishDate} Jobs finished", DateTime.Now);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Scheduler exited with error: {exception}");
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
