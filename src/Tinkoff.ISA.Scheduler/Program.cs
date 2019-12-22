using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Tinkoff.ISA.AppLayer;
using Tinkoff.ISA.AppLayer.Jobs;
using Tinkoff.ISA.DAL;
using Tinkoff.ISA.DAL.Common;
using Tinkoff.ISA.Infrastructure.Configuration;
using Tinkoff.ISA.Infrastructure.Extensions;
using Tinkoff.ISA.Infrastructure.Settings;
using Tinkoff.ISA.Providers.Jira;
using Tinkoff.ISA.Scheduler.Activators;
using Tinkoff.ISA.Scheduler.Schedule;

namespace Tinkoff.ISA.Scheduler
{
    internal class Program
    {
        private static readonly ManualResetEventSlim Shutdown = new ManualResetEventSlim();

        private static void Main()
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(ConfigHelper.GetConfigByEnvironment(environmentName));

            var configuration = builder.Build();

            var logSettings = configuration.GetSection("Logging").Get<LoggingSettings>();
            var jiraProviderSettings = JiraSettingsCreator.CreateProviderSettings(configuration);

            //setup our DI
            var services = new ServiceCollection()
                .AddSingleton(new LoggerFactory().AddSerilog(LogExtensions.CreateLogger(logSettings, "isa-.log")))
                .AddLogging()
                .AddJiraProvider(jiraProviderSettings)
                .AddSingleton<IJob, JiraJob>()
                .AddSingleton<IJob, ConfluenceJob>()
                .AddSingleton<IJob, MongoIndexingForElasticJob>()
                .AddSingleton<IScheduler, Schedule.Scheduler>()
                .AddSingleton<IJobsActivator, JobsActivator>()
                .AddAppDependencies()
                .AddDalDependencies()
                .AddInfrastructureDependencies()
                .Configure<SchedulerSettings>(configuration.GetSection("Scheduler"))
                .Configure<JiraSettings>(configuration.GetSection("JiraSettings"))
                .Configure<ConfluenceSettings>(configuration.GetSection("ConfluenceSettings"))
                .Configure<ElasticSearchSettings>(configuration.GetSection("ElasticsearchSettings"))
                .Configure<ConnectionStringsSettings>(configuration.GetSection("ConnectionStrings"));
            
            services.AddHttpClient<IHttpClient, HttpClientWrapper>();
            
            var serviceProvider = services.BuildServiceProvider();

            var logger = serviceProvider.GetService<ILoggerFactory>()
                .CreateLogger<Program>();
            
            logger.LogDebug("Starting application");
            Console.WriteLine("Starting application");
            
            using (serviceProvider.GetService<IScheduler>())
            {
                Shutdown.Wait();
            }
        }
    }
}
