using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Tinkoff.ISA.AppLayer.Questions;
using Tinkoff.ISA.AppLayer.Slack.Event.ButtonParams;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.ActionHandlers;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.ActionHandlers.Params;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.Request;
using Tinkoff.ISA.DAL.Slack;
using Tinkoff.ISA.DAL.Slack.Dtos;
using Tinkoff.ISA.Domain;
using Tinkoff.ISA.Infrastructure.Settings;
using Xunit;

namespace Tinkoff.ISA.AppLayer.UnitTests.Slack.ActionHandlers
{
    public class AskExpertsSlackActionHandlerTests
    {
        private const string UserId = "UBJ6GC75K";
        private const string UserName = "Bob";
        private readonly Mock<IQuestionService> _questionServiceMock;
        private readonly Mock<ISlackHttpClient> _slackClientMock;
        private readonly AskExpertsSlackActionHandler _handler;


        public AskExpertsSlackActionHandlerTests()
        {
            var slackSettings = new SlackSettings { ExpertsChannelId = "CBGJMA0TA" };
            var options = Options.Create(slackSettings);

            _questionServiceMock = new Mock<IQuestionService>();
            _slackClientMock = new Mock<ISlackHttpClient>();
            var logger = new Mock<ILogger<AskExpertsSlackActionHandler>>();
            _handler = new AskExpertsSlackActionHandler(_questionServiceMock.Object, _slackClientMock.Object, options,
                logger.Object);

            _slackClientMock
                .Setup(m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<AttachmentDto>>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ParamsIsNull_ArgumentNullException()
        {
            // Act, Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null));
        }

        [Fact]
        public async Task Handle_CorrectParamsTitleWithoutPhrase_ShouldSendQuestionToChannel()
        {
            // Arrange
            var questionId = Guid.NewGuid();

            var attachment = new AttachmentDto
            {
                Text = "questionText",
                Actions = new List<AttachmentActionDto> { new AttachmentActionDto("test", "test") },
            };

            var originalMessage = new OriginalMessageDto
            {
                Text = "testText",
                Attachments = new List<AttachmentDto> { attachment },
                TimeStamp = It.IsAny<string>()
            };

            var buttonParams = new AskExpertsActionButtonParams
            {
                QuestionText = "question text goes here"
            };

            var actionParams = new AskExpertsSlackActionParams
            {
                User = new ItemInfo
                {
                    Id = UserId,
                    Name = UserName
                },
                AttachmentId = 0,
                OriginalMessage = originalMessage,
                Channel = new ItemInfo{Id = "testChannel"},
                ButtonParams = buttonParams
            };
            
            var question = new Question { Text = "???", Id = questionId };

            _questionServiceMock
                .Setup(m => m.UpsertAsync(It.IsAny<Question>()))
                .ReturnsAsync(question);

            // Act
            await _handler.Handle(actionParams);

            // Assert
            _questionServiceMock.Verify(
                m => m.UpsertAsync(It.IsAny<Question>()), Times.Once);
            _questionServiceMock.VerifyNoOtherCalls();
            
            _slackClientMock.Verify(m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<AttachmentDto>>()));
            _slackClientMock.Verify(m => m.UpdateMessageAsync(actionParams.OriginalMessage.TimeStamp, actionParams.Channel.Id,
                It.IsAny<string>(), It.IsAny<IList<AttachmentDto>>()));
            _slackClientMock.VerifyNoOtherCalls();
        }
    }

}

