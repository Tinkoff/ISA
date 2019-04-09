using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Tinkoff.ISA.Infrastructure.Settings;
using Tinkoff.ISA.Scheduler.Activators;
using Xunit;

namespace Tinkoff.ISA.Scheduler.UnitTests.Scheduler
{
    public class SchedulerTests
    {
        private readonly Mock<IJobsActivator> _jobsActivatorMock;
        private readonly Mock<ILogger<Schedule.Scheduler>> _logger;
        private readonly Mock<IOptions<SchedulerSettings>> _schedulerSettingOptionsMock;

        public SchedulerTests()
        {
            _jobsActivatorMock = new Mock<IJobsActivator>();

            _logger = new Mock<ILogger<Schedule.Scheduler>>();

            _schedulerSettingOptionsMock = new Mock<IOptions<SchedulerSettings>>();
            _schedulerSettingOptionsMock
                .SetupGet(m => m.Value)
                .Returns(() => new SchedulerSettings()
                {
                    LoopTimeMinutes = 1
                });
        }

        [Fact]
        public async void StartJobs_JustInvoked_ShouldInvokeJobsActivatorStartJobs()
        {
            //act
            var scheduler = new Schedule.Scheduler(
                _logger.Object,
                _schedulerSettingOptionsMock.Object,
                _jobsActivatorMock.Object);

            await Task.Delay(1000);

            //assert
            _jobsActivatorMock.Verify(m => m.StartJobs(), Times.AtLeastOnce);
        }
    }
}
