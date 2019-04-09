using System.Linq;
using AutoMapper;
using Newtonsoft.Json;
using Tinkoff.ISA.AppLayer.Slack.Event.ButtonParams;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.ActionHandlers.Params;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.Request;

namespace Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.ActionHandlers.Mappings
{
    public class AddAnswerSlackActionParamsMapping : Profile
    {
        public AddAnswerSlackActionParamsMapping()
        {
            CreateMap<InteractiveMessage, AddAnswerSlackActionParams>()
                .ForMember(d => d.ButtonParams,
                    opt => opt.MapFrom(src =>
                        JsonConvert.DeserializeObject<AddAnswerActionButtonParams>(src.Actions.First().Value)));
        }
    }
}
