using System.Linq;
using AutoMapper;
using Newtonsoft.Json;
using Tinkoff.ISA.AppLayer.Slack.Event.ButtonParams;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.ActionHandlers.Params;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.Request;

namespace Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.ActionHandlers.Mappings
{
    public class AnswerSlackActionParamsMapping : Profile
    {
        public AnswerSlackActionParamsMapping()
        {
            CreateMap<InteractiveMessage, AnswerSlackActionParams>()
                .ForMember(d => d.ButtonParams,
                    opt => opt.MapFrom(src =>
                        JsonConvert.DeserializeObject<AnswerActionButtonParams>(src.Actions.First().Value)));
        }
    }
}
