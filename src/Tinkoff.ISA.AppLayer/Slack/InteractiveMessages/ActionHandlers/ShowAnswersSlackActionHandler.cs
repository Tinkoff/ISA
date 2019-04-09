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
    internal class ShowAnswersSlackActionHandler : ISlackActionHandler<ShowAnswersSlackActionParams>
    {
        private readonly IQuestionService _questionService;
        private readonly ISlackHttpClient _slackClient;
        private readonly ILogger<ShowAnswersSlackActionHandler> _logger;

        public ShowAnswersSlackActionHandler(IQuestionService questionService, ISlackHttpClient slackClient,
            ILogger<ShowAnswersSlackActionHandler> logger)
        {
            _questionService = questionService;
            _slackClient = slackClient;
            _logger = logger;
        }

        public async Task Handle(ShowAnswersSlackActionParams actionParams)
        {
            if (actionParams == null) throw new ArgumentNullException(nameof(actionParams));
            if (actionParams.ButtonParams == null) throw new ArgumentNullException(nameof(actionParams.ButtonParams));

            _logger.LogInformation(
                "User {User} with id {UserId} looked at the answers to the question {QuestionId}",
                actionParams.User.Name, actionParams.User.Id,
                actionParams.ButtonParams.QuestionId);

            var question = await _questionService.GetQuestionAsync(actionParams.ButtonParams.QuestionId);
            var bestAnswer = question.Answers.OrderByDescending(r => r.Rank).FirstOrDefault();

            var attachment = CreateAttachment(question, bestAnswer);
            var attachments = actionParams.OriginalMessage.Attachments;

            attachments[actionParams.AttachmentId] = attachment;

            await _slackClient.UpdateMessageAsync(
                actionParams.OriginalMessage.TimeStamp, 
                actionParams.Channel.Id, 
                actionParams.OriginalMessage.Text,
                attachments);
        }

        private static AttachmentDto CreateAttachment(Question question, Answer bestAnswer)
        {
            return new AttachmentDto
            {
                Color = Color.LightSkyBlue,
                Text = $"{Phrases.QuestionInfoText}{question.Text}\n\n\n{bestAnswer?.Text}\n\n\n{Phrases.RatingOfAnswer}{bestAnswer?.Rank}\n_{Phrases.AskExperts}_",
                CallbackId = CallbackId.StandardButtonsId,
                Actions = CreateButtons(question, bestAnswer)
            };
        }

        private static IList<AttachmentActionDto> CreateButtons(Question question, Answer answer)
        {
            var buttonParams = JsonConvert.SerializeObject(new
            {
                QuestionId = question.Id,
                AnswerId = answer.Id
            });

            return new List<AttachmentActionDto>
            {
                new HelpedButtonAttachmentAction(buttonParams),
                new NotHelpedButtonAttachmentAction(buttonParams),
                new ShowMoreAnswersButtonAttachmentAction(buttonParams),
                new AskExpertsButtonAttachmentAction(question.Text)
            };
        }
    }
}
