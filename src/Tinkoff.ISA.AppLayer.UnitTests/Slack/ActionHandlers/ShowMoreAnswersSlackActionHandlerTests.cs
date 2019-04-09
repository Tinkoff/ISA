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
using Tinkoff.ISA.Domain;
using Xunit;

namespace Tinkoff.ISA.AppLayer.UnitTests.Slack.ActionHandlers
{
    public class ShowMoreAnswersSlackActionHandlerTests
    {
        private readonly Mock<IQuestionService> _answerService;
        private readonly Mock<ISlackHttpClient> _slackClient;
        private readonly ShowMoreAnswersSlackActionHandler _handler;

        public ShowMoreAnswersSlackActionHandlerTests()
        {
            _answerService = new Mock<IQuestionService>();
            _slackClient = new Mock<ISlackHttpClient>();
            var logger = new Mock<ILogger<ShowMoreAnswersSlackActionHandler>>();
            _handler = new ShowMoreAnswersSlackActionHandler(_answerService.Object, _slackClient.Object, logger.Object);
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
            await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(new ShowMoreAnswersSlackActionParams()));
        }

        [Fact]
        public async Task Handle_CorrectParams_ShouldSendAllOtherAnswersToChannel()
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

            var actionParams = new ShowMoreAnswersSlackActionParams()
            {
                User = new ItemInfo
                {
                    Id = "id",
                    Name = "Bob"
                },
                OriginalMessage = originalMessage,
                Channel = new ItemInfo {Id = "channelId", Name = "channelName"},
                ButtonParams = new ShowMoreAnswersActionButtonParams()
                {
                    AnswerId = "1234",
                    QuestionId = "1234"
                }
            };

            var answers = new List<Answer>
            {
                new Answer
                {
                    Id = new Guid(),
                    Rank = 123,
                    Text = "123"
                },
                new Answer
                {
                    Id = new Guid(),
                    Rank = 456,
                    Text = "456"
                }
            };

            _answerService
                .Setup(m => m.GetAnswersOnQuestionExceptAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(answers);

            _slackClient
                .Setup(m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<AttachmentDto>>()))
                .Returns(Task.CompletedTask);
            _slackClient
                .Setup(m => m.UpdateMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<AttachmentDto>>()))
                .Returns(Task.CompletedTask);


            // Act
            await _handler.Handle(actionParams);

            // Assert
            _answerService.Verify(
                m => m.GetAnswersOnQuestionExceptAsync(actionParams.ButtonParams.QuestionId,
                    actionParams.ButtonParams.QuestionId), Times.Once);
            _answerService.VerifyNoOtherCalls();
            _slackClient.Verify(
                m => m.SendMessageAsync(actionParams.Channel.Id, It.IsAny<string>(), It.IsAny<List<AttachmentDto>>()), Times.Once);
            _slackClient.Verify(m => m.UpdateMessageAsync(actionParams.OriginalMessage.TimeStamp, actionParams.Channel.Id,
                It.IsAny<string>(), It.IsAny<List<AttachmentDto>>()));
            _slackClient.VerifyNoOtherCalls();
        }
    }
}
