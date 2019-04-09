using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tinkoff.ISA.AppLayer.Jobs;
using Tinkoff.ISA.Infrastructure.Settings;

namespace Tinkoff.ISA.Scheduler.Activators
{
    internal class JobsActivator : IJobsActivator
    {
        private static readonly Type JobsInterfaceType = typeof(IJob);
        private static string[] _jobNames;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<JobsActivator> _logger;
        private readonly IOptions<SchedulerSettings> _settings;

        public JobsActivator(
            IServiceProvider serviceProvider,
            IOptions<SchedulerSettings> settings,
            ILogger<JobsActivator> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _settings = settings;
            _jobNames = _settings.Value.JobNames;
        }

        public async void StartJobs()
        {
            var jobServices = _serviceProvider.GetServices(JobsInterfaceType).ToList();
            var jobTypes = GetJobsTypes(JobsInterfaceType);
            var methods = JobsInterfaceType.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            foreach (var jobType in jobTypes)
            {
                var job = jobServices.FirstOrDefault(o => o.GetType() == jobType);

                for (var i = 0; i < _settings.Value.MaxRetries; i++)
                {
                    try
                    {
                        _logger.LogInformation("{JobName} started {LaunchDate}", jobType.Name, DateTime.Now);
                        await (Task)jobType.InvokeMember(methods.First().Name, BindingFlags.InvokeMethod, null, job, null);
                        _logger.LogInformation("{JobName} finished {FinishDate}", jobType.Name, DateTime.Now);
                        break;
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning("{JobName} exited with error: {exception}", jobType.Name, e.Message);
                        await Task.Delay(_settings.Value.RestartJobDelayMilliseconds);
                        _logger.LogInformation("{JobName} relaunch attempt № {tryNumber}", jobType.Name, i + 1);
                    }
                }
            }
        }

        private static IEnumerable<Type> GetJobsTypes(Type interfaceType)
        {
            return AppDomain.CurrentDomain
                    .GetAssemblies()
                    .SelectMany(x => x.GetTypes())
                    .Where(x =>
                        interfaceType.IsAssignableFrom(x) &&
                        _jobNames.Contains(x.Name) &&
                        !x.IsInterface &&
                        !x.IsAbstract)
                    .ToList();
        }
    }
}
