namespace Tinkoff.ISA.Infrastructure.Settings
{
    public class ConfluenceSettings
    {
        public string User { get; set; }

        public string Password { get; set; }

        public int BatchSize { get; set; }

        public string BaseAddress { get; set; }

        public long HttpTimeoutMilliseconds { get; set; }

        public string[] SpaceKeys { get; set; }
    }
}
