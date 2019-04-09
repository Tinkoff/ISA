using Serilog.Events;

namespace Tinkoff.ISA.Infrastructure.Settings
{
    public class LoggingSettings
    {
        public string LogsFolder { get; set; }

        public LogEventLevel SerilogLogLevel { get; set; }

        public LogEventLevel SystemLogLevel { get; set; }
    }
}
