using System.IO;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Mongo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tinkoff.ISA.DAL;
using Tinkoff.ISA.Infrastructure.Configuration;
using Tinkoff.ISA.Infrastructure.Extensions;
using Tinkoff.ISA.Infrastructure.MongoDb;
using Tinkoff.ISA.Infrastructure.Settings;

namespace Tinkoff.ISA.SchedulerUI
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
            services.AddInfrastructureDependencies();
            services.AddDalDependencies();
            services.AddHangfire((serviceProvider, config) =>
            {
                var mongoContext = serviceProvider
                    .GetService<IMongoContext>();

                config.UseMongoStorage(
                    mongoContext.MongoClient.Settings,
                    "Jobs",
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
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
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

            app.UseHangfireDashboard();

            app.Run(context => {
                context.Response.Redirect("/hangfire");
                return Task.CompletedTask;
            });
        }
    }
}
