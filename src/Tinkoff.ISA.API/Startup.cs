using System.IO;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tinkoff.ISA.AppLayer;
using Tinkoff.ISA.Infrastructure.Settings;
using Tinkoff.ISA.DAL;
using Tinkoff.ISA.Infrastructure.Configuration;
using Tinkoff.ISA.Infrastructure.Extensions;
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
            services.Configure<ElasticsearchSettings>(_configuration.GetSection("ElasticsearchSettings"));
            services.AddInfrastructureDependencies();
            services.AddDalDependencies();
            services.AddAppDependencies();
            services.AddMvc();
            services.AddAutoMapper();
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

            app.UseMvc();
        }
    }
}
