using System.Collections.Generic;
using Newtonsoft.Json;

namespace Tinkoff.ISA.DAL.Slack.Dtos
{
    public class OriginalMessageDto
    {
        public string Text { get; set; }

        [JsonProperty("ts")]
        public string TimeStamp { get; set; }

        public IList<AttachmentDto> Attachments { get; set; }
    }
}
