namespace Tinkoff.ISA.Infrastructure.Settings
{
    public class JiraSettings
    {
        public string BaseAddress { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public string[] ProjectNames { get; set; }

        public int BatchSize { get; set; }
    }
}
