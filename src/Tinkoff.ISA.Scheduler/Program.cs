﻿using System.Threading.Tasks;
using Hangfire;
using Hangfire.Mongo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Tinkoff.ISA.AppLayer;
using Tinkoff.ISA.AppLayer.Jobs;
using Tinkoff.ISA.DAL;
using Tinkoff.ISA.Infrastructure.Configuration;
using Tinkoff.ISA.Infrastructure.Extensions;
using Tinkoff.ISA.Infrastructure.MongoDb;
using Tinkoff.ISA.Infrastructure.Settings;
using Tinkoff.ISA.Providers.Jira;
using Tinkoff.ISA.Scheduler.Jobs;

namespace Tinkoff.ISA.Scheduler
{
    internal static class Program
    {
        public static Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;
                    config
                        .SetBasePath(env.ContentRootPath)
                        .AddJsonFile(ConfigHelper.GetConfigByEnvironment(env.EnvironmentName));
                })
                .ConfigureServices((context, services) =>
                {
                    var configuration = context.Configuration;

                    var jiraProviderSettings = JiraSettingsCreator.CreateProviderSettings(configuration);

                    services
                        .AddHangfire((serviceProvider, config) =>
                        {
                            var mongoContext = serviceProvider
                                .GetService<IMongoContext>();

                            config.UseMongoStorage(
                                mongoContext.MongoClient.Settings,
                                "HangfireJobs",
                                new MongoStorageOptions
                                {
                                    MigrationOptions = new MongoMigrationOptions
                                    {
                                        Strategy = MongoMigrationStrategy.Drop,
                                        BackupStrategy = MongoBackupStrategy.Collections
                                    }
                                });
                        })
                        .AddJiraProvider(jiraProviderSettings)
                        .AddSingleton<IHostedService, HangfireService>()
                        .AddAppDependencies()
                        .AddDalDependencies()
                        .AddInfrastructureDependencies()
                        .Configure<ConnectionStringsSettings>(configuration.GetSection("ConnectionStrings"))
                        .Configure<JiraSettings>(configuration.GetSection("JiraSettings"))
                        .Configure<ConfluenceSettings>(configuration.GetSection("ConfluenceSettings"))
                        .Configure<ElasticSearchSettings>(configuration.GetSection("ElasticsearchSettings"))
                        .AddSingleton<IJob, JiraJob>()
                        .AddSingleton<IJob, ConfluenceJob>()
                        .AddSingleton<IJob, MongoIndexingForElasticJob>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    var logSettings = hostingContext.Configuration
                        .GetSection("Logging")
                        .Get<LoggingSettings>();
                    var logger = LogExtensions.CreateLogger(logSettings, "isa-.log");
                    
                    logging.AddSerilog(logger);
                })
                .UseConsoleLifetime()
                .Build();

            host.Services.AddJobs();
            
            return host.RunAsync();
        }
    }
}
