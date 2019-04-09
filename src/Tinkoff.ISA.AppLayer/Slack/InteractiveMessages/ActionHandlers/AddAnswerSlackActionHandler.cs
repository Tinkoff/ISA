using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tinkoff.ISA.AppLayer.Slack.Common;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.ActionHandlers.Params;
using Tinkoff.ISA.DAL.Slack;
using Tinkoff.ISA.DAL.Slack.Dtos;

namespace Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.ActionHandlers
{
    internal class AddAnswerSlackActionHandler : ISlackActionHandler<AddAnswerSlackActionParams>
    {
        private readonly ISlackHttpClient _slackClient;
        private readonly ILogger<AddAnswerSlackActionHandler> _logger;
        private readonly ICallbackIdCustomParamsWrappingService _callbackIdCustomParamsWrappingService;

        public AddAnswerSlackActionHandler(ISlackHttpClient slackClient, ILogger<AddAnswerSlackActionHandler> logger,
            ICallbackIdCustomParamsWrappingService callbackIdCustomParamsWrappingService)
        {
            _slackClient = slackClient;
            _logger = logger;
            _callbackIdCustomParamsWrappingService = callbackIdCustomParamsWrappingService;
        }

        public Task Handle(AddAnswerSlackActionParams actionParams)
        {
            if (actionParams == null) throw new ArgumentNullException(nameof(actionParams));
            if (actionParams.User == null) throw new ArgumentNullException(nameof(actionParams.User));
            if (actionParams.ButtonParams == null) throw new ArgumentNullException(nameof(actionParams.ButtonParams));

            var questionId = actionParams.ButtonParams.QuestionId;
            var triggerId = actionParams.TriggerId;
            if (string.IsNullOrEmpty(questionId)) throw new ArgumentException(nameof(questionId));
            if (string.IsNullOrEmpty(triggerId)) throw new ArgumentException(nameof(triggerId));

            _logger.LogInformation(
                "User {User} with id {UserId} opened a dialog box to add an answer to the question {QuestionId}",
                actionParams.User.Name, actionParams.User.Id, questionId);

            var packedParams =
                _callbackIdCustomParamsWrappingService.Wrap(CallbackId.AddAnswerDialogSubmissionId,
                    new[] {questionId});

            var dialog = new DialogRequest
            {
                TriggerId = actionParams.TriggerId,
                Dialog = new DialogDto
                {
                    CallbackId = packedParams,
                    Title = "Add new answer",
                    State = actionParams.OriginalMessage.TimeStamp,
                    Elements = new List<DialogElementDto>
                    {
                        new DialogElementDto
                        {
                            Label = "Your answer",
                            Name = "experts_answer",
                            Type = "textarea"
                        }
                    }
                }
            };

            return _slackClient.OpenDialogAsync(dialog);
        }
    }
}
