using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Tinkoff.ISA.Infrastructure.Logging;
using Tinkoff.ISA.Infrastructure.Settings;

namespace Tinkoff.ISA.Infrastructure.Extensions
{
    public static class LogExtensions
    {
        public static IApplicationBuilder UseSerilog(this IApplicationBuilder app, ILoggerFactory loggerFactory,
            LoggingSettings settings, string logFileName)
        {
            var logger = CreateLogger(settings, logFileName);
            loggerFactory.AddSerilog(logger, true);
            return app;
        }

        public static Logger CreateLogger(LoggingSettings settings, string logFileName)
        {
            var path = Path.Combine(Path.GetFullPath(settings.LogsFolder), logFileName);

            return new LoggerConfiguration()
                .MinimumLevel.Is(settings.SerilogLogLevel)
                .MinimumLevel.Override("Microsoft", settings.SystemLogLevel)
                .MinimumLevel.Override("System", settings.SystemLogLevel)
                .WriteTo.Async(a => a.File(new RawJsonFormatter(), path,
                    rollingInterval: RollingInterval.Hour, 
                    shared: true))
                .Enrich.FromLogContext()
                .CreateLogger();
        }
    }
}