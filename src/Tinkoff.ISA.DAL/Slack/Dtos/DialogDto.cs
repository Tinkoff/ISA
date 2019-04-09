using System.Collections.Generic;

namespace Tinkoff.ISA.DAL.Slack.Dtos
{
    public class DialogDto
    {
        public string CallbackId { get; set; }

        public string Title { get; set; }

        public string SubmitLabel { get; set; }

        public string State { get; set; }

        public IList<DialogElementDto> Elements { get; set; }
    }
}
