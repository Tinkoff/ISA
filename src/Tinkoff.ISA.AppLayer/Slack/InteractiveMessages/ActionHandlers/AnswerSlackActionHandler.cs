using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Tinkoff.ISA.AppLayer.Questions;
using Tinkoff.ISA.AppLayer.Slack.Buttons;
using Tinkoff.ISA.AppLayer.Slack.Common;
using Tinkoff.ISA.AppLayer.Slack.Event.ButtonParams;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.ActionHandlers.Params;
using Tinkoff.ISA.DAL.Slack;
using Tinkoff.ISA.DAL.Slack.Dtos;

namespace Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.ActionHandlers
{
    internal class AnswerSlackActionHandler : ISlackActionHandler<AnswerSlackActionParams>
    {

        private readonly ISlackHttpClient _slackClient;
        private readonly IQuestionService _questionService;
        private readonly ILogger<AnswerSlackActionHandler> _logger;

        public AnswerSlackActionHandler(ISlackHttpClient slackClient, IQuestionService questionService,
            ILogger<AnswerSlackActionHandler> logger)
        {
            _slackClient = slackClient;
            _questionService = questionService;
            _logger = logger;
        }

        public async Task Handle(AnswerSlackActionParams actionParams)
        {
            if (actionParams == null) throw new ArgumentNullException(nameof(actionParams));
            if (actionParams.ButtonParams == null) throw new ArgumentNullException(nameof(actionParams.ButtonParams));
            if (actionParams.User == null) throw new ArgumentNullException(nameof(actionParams.User));
            var questionId = actionParams.ButtonParams.QuestionId;
            if (string.IsNullOrEmpty(questionId)) throw new ArgumentException(nameof(questionId));

            var userId = actionParams.User.Id;
            var userName = actionParams.User.Name;

            _logger.LogInformation("User {User} with id {UserId} is going to add answer to the question {QuestionId}.",
                userName, userId, questionId);

            var question = await _questionService.GetQuestionAsync(questionId);
            if (question == null)
                throw new ArgumentNullException(nameof(question));

            var attachments = new List<AttachmentDto>();
            string messageText;

            if (question.Answers != null)
            {
                messageText = $"To the question:\n{question.Text}\n\nthe following answers were found:";
                attachments.AddRange(
                    question.Answers.OrderByDescending(a => a.Rank)
                        .Select(answer => new AttachmentDto
                        {
                            Text = $"{answer.Text}\n{Phrases.RatingOfAnswer}{answer.Rank}",
                            Color = Color.PictonBlue
                        }));
            }
            else
            {
                messageText = $"To the question:\n{question.Text}\n\nNo answer found. You can be the first";
            }

            var addAnswerParams = JsonConvert.SerializeObject(new AddAnswerActionButtonParams {QuestionId = questionId});
            attachments.Add(new AttachmentDto
            {
                CallbackId = CallbackId.AddAnswerButtonId,
                Color = Color.Sand,
                Actions = new List<AttachmentActionDto>
                {
                    new AddAnswerButtonAttachmentAction(addAnswerParams)
                }
            });

            var channel = await _slackClient.OpenDirectMessageChannelAsync(userId);
            await _slackClient.SendMessageAsync(channel.Id, messageText, attachments);
        }
    }
}
