using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tinkoff.ISA.AppLayer.Questions;
using Tinkoff.ISA.AppLayer.Slack.Buttons;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.ActionHandlers.Params;
using Tinkoff.ISA.DAL.Slack;

namespace Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.ActionHandlers
{
    public class StopNotifyUserSlackActionHandler : ISlackActionHandler<StopNotificationsSlackActionParams>
    {
        private readonly IQuestionService _questionService;
        private readonly ISlackHttpClient _slackClient;
        private readonly ILogger<StopNotifyUserSlackActionHandler> _logger;
        
        public StopNotifyUserSlackActionHandler(ISlackHttpClient slackClient, 
            IQuestionService questionService, 
            ILogger<StopNotifyUserSlackActionHandler> logger)
        {
            _slackClient = slackClient;
            _questionService = questionService;
            _logger = logger;
        }
        
        public async Task Handle(StopNotificationsSlackActionParams actionParams)
        {
            if (actionParams == null) throw new ArgumentNullException(nameof(actionParams));
            if (actionParams.ButtonParams == null) throw new ArgumentNullException(nameof(actionParams.ButtonParams));

            var questionId = actionParams.ButtonParams.QuestionId;
            var userId = actionParams.User.Id;
                            
            await _questionService.UnsubscribeNotificationForUser(questionId, userId);
            
            _logger.LogInformation(
                "User {User} with id {UserId} unsubscribed from the notifications for the question {QuestionId}",
                actionParams.User.Name, userId, questionId);

            await UpdateMessage(actionParams);
        }
        
        private Task UpdateMessage(StopNotificationsSlackActionParams actionParams)
        {
            var attachments = actionParams.OriginalMessage.Attachments;
            var updateAttachment = attachments[actionParams.AttachmentId];

            updateAttachment.Text = "_You have unsubscribed from the notifications for this question._";
            updateAttachment.Actions = updateAttachment.Actions
                .Where(t => t.Name != StopNotificationsButtonAttachmentAction.ActionName)
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