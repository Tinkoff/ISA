using System.Linq;
using AutoMapper;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.ActionHandlers.Params;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.Request;

namespace Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.ActionHandlers.Mappings
{
    public class AskExpertsSlackActionParamsMapping : Profile
    {
        public AskExpertsSlackActionParamsMapping()
        {
            CreateMap<InteractiveMessage, AskExpertsSlackActionParams>()
                .ForMember(d => d.AttachmentId, opt => opt.MapFrom(src => src.AttachmentId - 1))
                .ForPath(d => d.ButtonParams.QuestionText,
                    opt => opt.MapFrom(src => src.Actions.First().Value));
        }
    }
}
