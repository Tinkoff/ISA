using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tinkoff.ISA.AppLayer.Questions;
using Tinkoff.ISA.AppLayer.Slack.Buttons;
using Tinkoff.ISA.AppLayer.Slack.Common;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.ActionHandlers.Params;
using Tinkoff.ISA.DAL.Slack;

namespace Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.ActionHandlers
{
    internal class HelpedSlackActionHandler : ISlackActionHandler<HelpedSlackActionParams>
    {
        private readonly ISlackHttpClient _slackClient;
        private readonly IQuestionService _questionService;
        private readonly ILogger<HelpedSlackActionHandler> _logger;

        public HelpedSlackActionHandler(ISlackHttpClient slackClient, IQuestionService questionService,
            ILogger<HelpedSlackActionHandler> logger)
        {
            _slackClient = slackClient;
            _questionService = questionService;
            _logger = logger;
        }

        public async Task Handle(HelpedSlackActionParams actionParams)
        {
            if (actionParams == null) throw new ArgumentNullException(nameof(actionParams));
            if (actionParams.ButtonParams == null) throw new ArgumentNullException(nameof(actionParams.ButtonParams));

            _logger.LogInformation("User {User} with id {UserId} positively rated the answer {AnswerId} to the question {QuestionId}",
                actionParams.User.Name, actionParams.User.Id, actionParams.ButtonParams.AnswerId,
                actionParams.ButtonParams.QuestionId);

            await _questionService.AnswerRankUpAsync(actionParams.ButtonParams.QuestionId, actionParams.ButtonParams.AnswerId);

            await UpdateMessage(actionParams);
        }

        private Task UpdateMessage(HelpedSlackActionParams actionParams)
        {
            var attachments = actionParams.OriginalMessage.Attachments;
            var updateAttachment = attachments[actionParams.AttachmentId];

            updateAttachment.Text += $"\n:heavy_check_mark: {Phrases.ThanksForOpinion}";
            updateAttachment.Actions = updateAttachment.Actions
                .Where(t => t.Name != HelpedButtonAttachmentAction.ActionName &&
                            t.Name != NotHelpedButtonAttachmentAction.ActionName)
                .ToList();

            attachments[actionParams.AttachmentId] = updateAttachment;

            return _slackClient.UpdateMessageAsync(
                actionParams.OriginalMessage.TimeStamp,
                actionParams.Channel.Id,
                actionParams.OriginalMessage.Text,
                attachments);
        }
    }
}
