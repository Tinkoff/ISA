using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Tinkoff.ISA.AppLayer.Questions;
using Tinkoff.ISA.AppLayer.Slack.Buttons;
using Tinkoff.ISA.AppLayer.Slack.Common;
using Tinkoff.ISA.AppLayer.Slack.Event.ButtonParams;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.ActionHandlers.Params;
using Tinkoff.ISA.DAL.Slack;
using Tinkoff.ISA.DAL.Slack.Dtos;
using Tinkoff.ISA.Domain;
using Tinkoff.ISA.Infrastructure.Settings;

namespace Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.ActionHandlers
{
    internal class AskExpertsSlackActionHandler : ISlackActionHandler<AskExpertsSlackActionParams>
    {
        private readonly IQuestionService _questionService;
        private readonly ISlackHttpClient _slackClient;
        private readonly ILogger<AskExpertsSlackActionHandler> _logger;
        private readonly string _expertsChannelId;

        public AskExpertsSlackActionHandler(
            IQuestionService questionService,
            ISlackHttpClient slackClient,
            IOptions<SlackSettings> slackSettings,
            ILogger<AskExpertsSlackActionHandler> logger)
        {
            _questionService = questionService;
            _slackClient = slackClient;
            _logger = logger;
            _expertsChannelId = slackSettings.Value.ExpertsChannelId;
        }

        public async Task Handle(AskExpertsSlackActionParams actionParams)
        {
            if (actionParams == null) throw new ArgumentNullException(nameof(actionParams));
            var askedUserId = actionParams.User.Id;
            var question = new Question
            {
                Text = actionParams.ButtonParams.QuestionText.Trim('\n', ' '),
                AskedUsersIds = new List<string> { askedUserId }
            };

            _logger.LogInformation("User {User} with id {UserId} asked the experts. Question: {Question}",
                actionParams.User.Name, askedUserId, question.Id);
            
            await UpdateMessage(actionParams);
            
            question = await _questionService.UpsertAsync(question);
            var messageText = $"*<@{askedUserId}> asked the following question:*\n" +
                              $"_{question.Text}_";
            var attachments = CreateAttachments(question);

            await _slackClient.SendMessageAsync(_expertsChannelId, messageText, attachments);
        }

        private static List<AttachmentDto> CreateAttachments(Question question)
        {
            var answerParams = JsonConvert.SerializeObject(new AnswerActionButtonParams
            {
                QuestionId = question.Id.ToString()
            });

            return new List<AttachmentDto>
            {
                new AttachmentDto
                {
                    Color = Color.Sand,
                    CallbackId = CallbackId.AnswerButtonId,
                    Text = "_When you click on the answer button, go to private messages with ISA_",
                    Actions = new List<AttachmentActionDto> { new AnswerButtonAttachmentAction(answerParams) }
                }
            };
        }

        private Task UpdateMessage(AskExpertsSlackActionParams actionParams)
        {
            var attachments = actionParams.OriginalMessage.Attachments;
            var updateAttachment = attachments[actionParams.AttachmentId];

            updateAttachment.Text += $"\n{Phrases.SendToExperts}";
            updateAttachment.Actions = updateAttachment.Actions
                    .Where(t => t.Name != AskExpertsButtonAttachmentAction.ActionName)
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
