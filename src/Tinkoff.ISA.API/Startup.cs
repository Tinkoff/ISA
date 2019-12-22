using System.IO;
using AutoMapper;
using Hangfire;
using Hangfire.Mongo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tinkoff.ISA.AppLayer;
using Tinkoff.ISA.Infrastructure.Settings;
using Tinkoff.ISA.DAL;
using Tinkoff.ISA.DAL.Common;
using Tinkoff.ISA.Infrastructure.Configuration;
using Tinkoff.ISA.Infrastructure.Extensions;
using Tinkoff.ISA.Infrastructure.MongoDb;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Tinkoff.ISA.API
{
    public class Startup
    {
        private readonly IConfigurationRoot _configuration;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(ConfigHelper.GetConfigByEnvironment(env.EnvironmentName), false, true);

            _configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ConnectionStringsSettings>(_configuration.GetSection("ConnectionStrings"));
            services.Configure<SlackSettings>(_configuration.GetSection("SlackSettings"));
            services.Configure<ElasticSearchSettings>(_configuration.GetSection("ElasticsearchSettings"));
            services.AddInfrastructureDependencies();
            services.AddDalDependencies();
            services.AddHttpClient<IHttpClient, HttpClientWrapper>();
            services.AddAppDependencies();
            services.AddMvc();
            services.AddAutoMapper();
            services.AddHangfire((serviceProvider, config) =>
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
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app, 
            IHostingEnvironment env, 
            ILoggerFactory loggerFactory, 
            IRecurringJobManager recurringJobManager)
        {
            var logSettings = _configuration.GetSection("Logging").Get<LoggingSettings>();

            app.UseSerilog(loggerFactory, logSettings, "isa-.log");
            app.UseLogException();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseMvc();
        }
    }
}
