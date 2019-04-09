namespace Tinkoff.ISA.DAL.Slack.Dtos
{
    public class DialogRequest
    {
        public string TriggerId { get; set; }

        public DialogDto Dialog { get; set; }
    }
}
