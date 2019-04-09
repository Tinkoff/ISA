using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Tinkoff.ISA.AppLayer.Slack.Common;
using Tinkoff.ISA.AppLayer.Slack.Executing;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.Request;
using Tinkoff.ISA.AppLayer.Slack.TypeChooser.DialogSubmission;
using Tinkoff.ISA.DAL.Slack;
using Tinkoff.ISA.Infrastructure.Settings;

namespace Tinkoff.ISA.AppLayer.Slack.Routing
{
    internal class RoutingService : IRoutingService
    {
        private readonly IInteractiveMessageService _interactiveMessageService;
        private readonly ISubmissionSelectService _submissionSelectService;
        private readonly ISlackExecutorService _slackExecutorService;
        private readonly ISlackHttpClient _slackHttpClient;
        private readonly JsonSerializerSettings _defaultSlackSerializerSettings;
        private readonly ILogger<RoutingService> _logger;

        public RoutingService(
            IInteractiveMessageService interactiveMessageService,
            ISubmissionSelectService submissionSelectService,
            ISlackExecutorService slackExecutorService,
            ISlackHttpClient slackHttpClient, ILogger<RoutingService> logger)
        {
            _interactiveMessageService = interactiveMessageService;
            _submissionSelectService = submissionSelectService;
            _slackExecutorService = slackExecutorService;
            _slackHttpClient = slackHttpClient;
            _logger = logger;

            _defaultSlackSerializerSettings = SlackSerializerSettings.DefaultSettings;
        }

        public async Task Route(string payload)
        {
            if (string.IsNullOrEmpty(payload)) throw new ArgumentException();

            var invocationPayload = JsonConvert.DeserializeObject<InvocationPayloadRequest>(payload, _defaultSlackSerializerSettings);
            try
            {
                await DefinePayloadTypeAndInvokeMethod(invocationPayload, payload);
            }
            catch (Exception e)
            {
                if (invocationPayload?.Channel?.Id != null)
                    await _slackHttpClient.SendMessageAsync(invocationPayload.Channel.Id, Phrases.SlackError);
                _logger.LogError(e, e.Message);
            }
        }

        private Task DefinePayloadTypeAndInvokeMethod(InvocationPayloadRequest invocation, string payload)
        {
            switch (invocation.Type)
            {
                case MessageType.InteractiveMessage:
                    var interactiveMessage = JsonConvert.DeserializeObject<InteractiveMessage>(payload, _defaultSlackSerializerSettings);
                    return _interactiveMessageService.ProcessRequest(interactiveMessage);
                case MessageType.DialogSubmission:
                    var submissionType = _submissionSelectService.Choose(invocation.CallbackId);
                    var dialog = JsonConvert.DeserializeObject(payload, submissionType, _defaultSlackSerializerSettings);
                    return _slackExecutorService.ExecuteSubmission(dialog.GetType(), dialog);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
