namespace Tinkoff.ISA.DAL.Slack.Dtos
{
    public class AttachmentActionDto
    {
        public string Type = "button";

        public string Style { get; set; }

        public string Value { get; set; }

        public AttachmentActionDto(string name, string text)
        {
            Name = name;
            Text = text;
        }

        public string Name { get; }

        public string Text { get; }
    }
}
