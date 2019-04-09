using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.Request
{
    public class InvocationPayloadRequest
    {
        public string Type { get; set; }

        public string CallbackId { get; set; }

        [BindRequired]
        public ItemInfo Channel { get; set; }

        [BindRequired]
        public ItemInfo User { get; set; }

        public int AttachmentId { get; set; }
                
        public string State { get; set; }
    }
}
