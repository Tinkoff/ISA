using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Tinkoff.ISA.AppLayer.Jobs;
using Tinkoff.ISA.Infrastructure.Settings;
using Xunit;

namespace Tinkoff.ISA.Scheduler.UnitTests.JobsActivator
{
    public class JobsActivatorTests
    {
        private readonly Mock<ILogger<Activators.JobsActivator>> _logger;
        private readonly Mock<IOptions<SchedulerSettings>> _settingOptionsMock;

        public JobsActivatorTests()
        {
            _logger = new Mock<ILogger<Activators.JobsActivator>>();
            _settingOptionsMock = new Mock<IOptions<SchedulerSettings>>();
        }

        public class JobA : IJob
        {
            public static bool WasInvoked { get; private set; }

            public Task StartJob()
            {
                WasInvoked = true;
                return Task.CompletedTask;
            }
        }
        public class JobB : IJob
        {
            public static bool WasInvoked { get; private set; }

            public Task StartJob()
            {
                WasInvoked = true;
                return Task.CompletedTask;
            }
        }

        public class JobC : IJob
        {
            public static bool WasInvoked { get; private set; }

            public Task StartJob()
            {
                WasInvoked = true;
                return Task.CompletedTask;
            }
        }

        public class JobError : IJob
        {
            public static int InvokedCount { get; private set; }

            public Task StartJob()
            {
                InvokedCount += 1;
                throw new Exception();
            }
        }

        [Fact]
        public void StartJobs_JustInvoked_ShouldInvokedJobsAandBandnotInvokedC()
        {
            //arrange
            var names = new[]
            {
                "JobA",
                "JobB"
            };

            _settingOptionsMock
                .SetupGet(m => m.Value)
                .Returns(() => new SchedulerSettings()
                {
                    JobNames = names,
                    LoopTimeMinutes = 1,
                    MaxRetries = 3
                });

            var serviceProvider = new ServiceCollection()
                .AddSingleton<IJob, JobA>()
                .AddSingleton<IJob, JobB>()
                .AddSingleton<IJob, JobC>()
                .BuildServiceProvider();

            var job = new Activators.JobsActivator(
                serviceProvider,
                _settingOptionsMock.Object,
                _logger.Object);

            //act
            job.StartJobs();

            //Assert
            Assert.True(JobA.WasInvoked);
            Assert.True(JobB.WasInvoked);
            Assert.False(JobC.WasInvoked);
        }

        [Fact]
        public void StartJobs_JustInvoked_ShouldRestartJob()
        {
            //arrange
            var names = new[]
            {
                "JobError"
            };

            _settingOptionsMock
                .SetupGet(m => m.Value)
                .Returns(() => new SchedulerSettings()
                {
                    JobNames = names,
                    LoopTimeMinutes = 1,
                    MaxRetries = 2
                });

            var serviceProvider = new ServiceCollection()
                .AddSingleton<IJob, JobError>()
                .BuildServiceProvider();

            var job = new Activators.JobsActivator(
                serviceProvider,
                _settingOptionsMock.Object,
                _logger.Object);

            //act
            job.StartJobs();

            //Assert
            Assert.Equal(2, JobError.InvokedCount);
        }
    }
}
