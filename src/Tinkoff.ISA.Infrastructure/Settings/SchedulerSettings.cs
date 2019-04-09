namespace Tinkoff.ISA.Infrastructure.Settings
{
    public class SchedulerSettings
    {
        public long LoopTimeMinutes { get; set; }

        public int MaxRetries { get; set; }

        public int RestartJobDelayMilliseconds { get; set; }

        public string[] JobNames { get; set; }
    }
}
