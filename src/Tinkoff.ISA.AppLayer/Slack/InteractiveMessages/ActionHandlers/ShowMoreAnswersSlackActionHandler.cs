using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Tinkoff.ISA.AppLayer.Questions;
using Tinkoff.ISA.AppLayer.Slack.Buttons;
using Tinkoff.ISA.AppLayer.Slack.Common;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.ActionHandlers.Params;
using Tinkoff.ISA.DAL.Slack;
using Tinkoff.ISA.DAL.Slack.Dtos;
using Tinkoff.ISA.Domain;

namespace Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.ActionHandlers
{
    internal class ShowMoreAnswersSlackActionHandler : ISlackActionHandler<ShowMoreAnswersSlackActionParams>
    {
        private readonly IQuestionService _questionService;
        private readonly ISlackHttpClient _slackClient;
        private readonly ILogger<ShowMoreAnswersSlackActionHandler> _logger;

        public ShowMoreAnswersSlackActionHandler(IQuestionService questionService, ISlackHttpClient slackClient,
            ILogger<ShowMoreAnswersSlackActionHandler> logger)
        {
            _questionService = questionService;
            _slackClient = slackClient;
            _logger = logger;
        }

        public async Task Handle(ShowMoreAnswersSlackActionParams actionParams)
        {
            if (actionParams == null) throw new ArgumentNullException(nameof(actionParams));
            if (actionParams.ButtonParams == null) throw new ArgumentNullException(nameof(actionParams.ButtonParams));
            
            _logger.LogInformation(
                "User {User} with id {UserId} asked for more answers to the question {QuestionId}",
                actionParams.User.Name, actionParams.User.Id,
                actionParams.ButtonParams.QuestionId);

            await UpdateMessage(actionParams);

            var answers = await _questionService.GetAnswersOnQuestionExceptAsync(
                actionParams.ButtonParams.QuestionId, 
                actionParams.ButtonParams.AnswerId);

            var attachmentTitle = actionParams.OriginalMessage.Attachments[actionParams.AttachmentId].Title;
            var answerText = answers.Count != 0
                ? $"{attachmentTitle}\n{Phrases.FoundNumberOfAnswers}{answers.Count}"
                : $"{attachmentTitle}{Phrases.NoMoreAnswers}";

            var attachments = new List<AttachmentDto>();
            attachments.AddRange(
                answers.Select(answer => CreateAttachment(actionParams.ButtonParams.QuestionId, answer)));

            await _slackClient.SendMessageAsync(
                actionParams.Channel.Id,
                answerText,
                attachments);
        }

        private static AttachmentDto CreateAttachment(string questionId, Answer answer)
        {
            return new AttachmentDto
            {
                Color = Color.LightSkyBlue,
                Text = $"_{answer.Text}_\n\n\n{Phrases.RatingOfAnswer}{answer.Rank}\n",
                CallbackId = CallbackId.RateAnswerButtonsId,
                Actions = CreateButtons(questionId, answer)
            };
        }

        private static IList<AttachmentActionDto> CreateButtons(string questionId, Answer answer)
        {
            var buttonParams = JsonConvert.SerializeObject(new 
            {
                QuestionId = questionId,
                AnswerId = answer.Id.ToString()
            });

            return new List<AttachmentActionDto>
            {
                new HelpedButtonAttachmentAction(buttonParams),
                new NotHelpedButtonAttachmentAction(buttonParams),
            };
        }

        private Task UpdateMessage(ShowMoreAnswersSlackActionParams actionParams)
        {
            var attachments = actionParams.OriginalMessage.Attachments;
            var updateAttachment = attachments[actionParams.AttachmentId];

            updateAttachment.Actions = updateAttachment.Actions
                    .Where(t => t.Name != ShowMoreAnswersButtonAttachmentAction.ActionName)
                    .ToList();

            updateAttachment.Text += $"\n{Phrases.ShowAnswers}";
            attachments[actionParams.AttachmentId] = updateAttachment;

            return _slackClient.UpdateMessageAsync(
                actionParams.OriginalMessage.TimeStamp,
                actionParams.Channel.Id,
                actionParams.OriginalMessage.Text,
                attachments);
        }
    }
}
