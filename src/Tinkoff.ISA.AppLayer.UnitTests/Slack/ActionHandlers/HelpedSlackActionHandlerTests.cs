using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Tinkoff.ISA.AppLayer.Questions;
using Tinkoff.ISA.AppLayer.Slack.Event.ButtonParams;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.ActionHandlers;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.ActionHandlers.Params;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.Request;
using Tinkoff.ISA.DAL.Slack;
using Tinkoff.ISA.DAL.Slack.Dtos;
using Xunit;

namespace Tinkoff.ISA.AppLayer.UnitTests.Slack.ActionHandlers
{
    public class HelpedSlackActionHandlerTests
    {
        private readonly Mock<ISlackHttpClient> _slackClient;
        private readonly Mock<IQuestionService> _questionService;
        private readonly HelpedSlackActionHandler _handler;

        public HelpedSlackActionHandlerTests()
        {
            _slackClient = new Mock<ISlackHttpClient>();
            _questionService = new Mock<IQuestionService>();
            var logger = new Mock<ILogger<HelpedSlackActionHandler>>();
            _handler = new HelpedSlackActionHandler(_slackClient.Object, _questionService.Object, logger.Object);
        }

        [Fact]
        public async Task Handle_ParamsAreNull_ArgumentNullException()
        {
            // Act-Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null));
        }

        [Fact]
        public async Task Handle_ButtonParamsAreNull_ArgumentNullException()
        {
            // Act-Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(new HelpedSlackActionParams()));
        }

        [Fact]
        public async Task Handle_CorrectParams_ShouldUpdateRankAndSendAnwer()
        {
            // Arrange

            var attachment = new AttachmentDto()
            {
                Actions = new List<AttachmentActionDto> { new AttachmentActionDto("test", "test") },
            };

            var originalMessage = new OriginalMessageDto()
            {
                Text = "testText",
                Attachments = new List<AttachmentDto> { attachment },
                TimeStamp = It.IsAny<string>()
            };

            var actionParams = new HelpedSlackActionParams()
            {
                User = new ItemInfo
                {
                    Id = "id",
                    Name = "Bob"
                },
                OriginalMessage = originalMessage,
                Channel = new ItemInfo {Id = "channelId", Name = "channelName"},
                ButtonParams = new HelpedAnswerActionButtonParams
                {
                    AnswerId = "1234",
                    QuestionId = "1234"
                }
            };


            _questionService
                .Setup(m => m.AnswerRankUpAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _slackClient
                .Setup(m => m.UpdateMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<AttachmentDto>>()))
                .Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(actionParams);

            // Assert
            _questionService.Verify(
                m => m.AnswerRankUpAsync(actionParams.ButtonParams.QuestionId, actionParams.ButtonParams.AnswerId), Times.Once);
            _questionService.VerifyNoOtherCalls();
            _slackClient.Verify(m => m.UpdateMessageAsync(actionParams.OriginalMessage.TimeStamp, actionParams.Channel.Id,
                It.IsAny<string>(), It.IsAny<IList<AttachmentDto>>()));
            _slackClient.VerifyNoOtherCalls();
        }
    }
}
