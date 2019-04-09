using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tinkoff.ISA.AppLayer.Questions;
using Newtonsoft.Json;
using Tinkoff.ISA.AppLayer.Slack.Buttons;
using Tinkoff.ISA.AppLayer.Slack.Common;
using Tinkoff.ISA.AppLayer.Slack.Dialogs.Submissions;
using Tinkoff.ISA.AppLayer.Slack.Event.ButtonParams;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.Request;
using Tinkoff.ISA.DAL.Slack;
using Tinkoff.ISA.DAL.Slack.Dtos;
using Tinkoff.ISA.Domain;

namespace Tinkoff.ISA.AppLayer.Slack.Dialogs
{
    internal class AddAnswerDialogSubmissionService : IDialogSubmissionService<DialogSubmission<AddAnswerSubmission>>
    {
        private readonly IQuestionService _questionService;      
        private readonly ISlackHttpClient _slackClient;
        private readonly ILogger<AddAnswerDialogSubmissionService> _logger;
        private readonly ICallbackIdCustomParamsWrappingService _callbackIdCustomParamsWrappingService;
        private const string SpeechBalloon = ":speech_balloon:";

        public AddAnswerDialogSubmissionService(IQuestionService questionService, ISlackHttpClient slackClient,
            ILogger<AddAnswerDialogSubmissionService> logger, ICallbackIdCustomParamsWrappingService callbackIdCustomParamsWrappingService)
        {
            _questionService = questionService;
            _slackClient = slackClient;
            _logger = logger;
            _callbackIdCustomParamsWrappingService = callbackIdCustomParamsWrappingService;
        }

        public async Task ProcessSubmission(DialogSubmission<AddAnswerSubmission> submission)
        {
            if (submission == null) throw new ArgumentNullException(nameof(submission));

            var customParams = _callbackIdCustomParamsWrappingService.Unwrap(submission.CallbackId);
            var answer = submission.Submission.ExpertsAnswer.Trim('\n', ' ');   
            var questionId = customParams.FirstOrDefault();
            
            if (questionId == null)
                throw new ArgumentNullException(nameof(questionId));
            
            await _questionService.AppendAnswerAsync(questionId, answer);
            LogNewAnswer(submission.User, questionId, answer);
            
            var question = await _questionService.GetQuestionAsync(questionId);

            await UpdateMessageForUser(answer, question.Text, submission).ConfigureAwait(false);

            await NotifyWatchers(question, answer, submission.User.Id).ConfigureAwait(false);
        }

        private void LogNewAnswer(ItemInfo user, string questionId, string answer)
        {
            _logger.LogInformation(
                "User {User} with id {UserId} added the answer to the knowledge base to the question {QuestionId} " +
                "Answer content: {Answer}",
                user.Name, user.Id,
                questionId, answer);
        }

        private Task UpdateMessageForUser(string answerText, string questionText, InvocationPayloadRequest request)
        {
            var message = $"{SpeechBalloon}\n" +
                          $"*Your answer:* _{answerText}_\n" +
                          $"*On question:* _{questionText}_\n" +
                          "*Your answer will be recorded in a moment. Thank you!*";
            
            return _slackClient.UpdateMessageAsync(
                request.State, 
                request.Channel.Id, 
                message);
        }

        private Task NotifyWatchers(Question question, string answerText, string answererId)
        {
            var message = $"{SpeechBalloon}\n" +
                          "*On your question*:\n" +
                          $"_{question.Text}_\n" +
                          $"*<@{answererId}> added new answer:*\n" +
                          $"_{answerText}_\n";
            
            var attachments = CreateAttachments(question.Id);
            
            return Task.WhenAll(question.AskedUsersIds
                .Where(x => x != answererId)
                .Select(x => SendNotificationForUser(x, message, attachments)));
        }
        
        private static List<AttachmentDto> CreateAttachments(Guid questionId)
        {
            var questionIdJson = JsonConvert.SerializeObject(new StopNotificationsActionButtonParams
            {
                QuestionId = questionId.ToString()
            });

            return new List<AttachmentDto>
            {
                new AttachmentDto
                {
                    Color = Color.Sand,
                    CallbackId = CallbackId.StopNotifications, 
                    Text = "_You can unsubscribe from notifications for this question_",
                    Actions = new List<AttachmentActionDto> { new StopNotificationsButtonAttachmentAction(questionIdJson) }
                }
            };
        }

        private async Task SendNotificationForUser(string userId, string message, IList<AttachmentDto> attachments)
        {
            var channel = await _slackClient.OpenDirectMessageChannelAsync(userId);
            await _slackClient.SendMessageAsync(channel.Id, message, attachments);
        }
    }
}
