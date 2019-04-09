using System.Collections.Generic;
using Newtonsoft.Json;
using Tinkoff.ISA.DAL.Slack.Dtos;

namespace Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.Request
{
    public class InteractiveMessage : InvocationPayloadRequest
    {
        public string TriggerId { get; set; }
            
        public IList<AttachmentActionDto> Actions { get; set; }

        public OriginalMessageDto OriginalMessage { get; set; }
    }
}
