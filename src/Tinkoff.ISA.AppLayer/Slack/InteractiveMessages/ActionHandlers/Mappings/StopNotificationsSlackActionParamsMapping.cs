using System.Linq;
using AutoMapper;
using Newtonsoft.Json;
using Tinkoff.ISA.AppLayer.Slack.Event.ButtonParams;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.ActionHandlers.Params;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.Request;

namespace Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.ActionHandlers.Mappings
{
    public class StopNotificationsSlackActionParamsMapping : Profile 
    {
        public StopNotificationsSlackActionParamsMapping()
        {
            CreateMap<InteractiveMessage, StopNotificationsSlackActionParams>()
                .ForMember(d => d.AttachmentId, opt => opt.MapFrom(src => src.AttachmentId - 1))
                .ForMember(d => d.ButtonParams,
                    opt => opt.MapFrom(src =>
                        JsonConvert.DeserializeObject<StopNotificationsActionButtonParams>(src.Actions.First().Value)));
        }
    }
}