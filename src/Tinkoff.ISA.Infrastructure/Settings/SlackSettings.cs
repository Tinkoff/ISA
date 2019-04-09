namespace Tinkoff.ISA.Infrastructure.Settings
{
    public class SlackSettings
    {
        public string BotToken { get; set; }

        public string ExpertsChannelId { get; set; }

        public string SigningSecret { get; set; }

        public string BaseAddress { get; set; }

        public long HttpTimeoutMilliseconds { get; set; }
    }
}
