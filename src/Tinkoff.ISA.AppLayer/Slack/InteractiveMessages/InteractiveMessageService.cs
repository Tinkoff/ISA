using System;
using System.Linq;
using System.Threading.Tasks;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.Request;
using AutoMapper;
using Tinkoff.ISA.AppLayer.Slack.Executing;
using Tinkoff.ISA.AppLayer.Slack.TypeChooser.ActionParams;

namespace Tinkoff.ISA.AppLayer.Slack.InteractiveMessages
{   
    internal class InteractiveMessageService : IInteractiveMessageService
    {
        private readonly ISlackParamsSelectService _slackParamsSelectService;
        private readonly ISlackExecutorService _slackExecutorService;
        private readonly IMapper _mapper;

        public InteractiveMessageService(
            ISlackParamsSelectService slackParamsSelectService,
            ISlackExecutorService slackExecutorService,
            IMapper mapper)
        {
            _slackParamsSelectService = slackParamsSelectService;
            _slackExecutorService = slackExecutorService;
            _mapper = mapper;
        }

        public Task ProcessRequest(InteractiveMessage request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var invokedAction = request.Actions.FirstOrDefault();
            if (invokedAction == null) throw new ArgumentNullException(nameof(invokedAction));

            var paramsType = _slackParamsSelectService.Choose(invokedAction.Name);
            var actionParams = _mapper.Map(request, request.GetType(), paramsType);
            return _slackExecutorService.ExecuteAction(paramsType, actionParams);
        }
    }
}
