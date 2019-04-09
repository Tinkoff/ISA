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
    public class AnswerSlackActionHandlerTests
    {
        private readonly Mock<IQuestionService> _questionServiceMock;
        private readonly Mock<ISlackHttpClient> _slackClientMock;
        private readonly AnswerSlackActionHandler _handler;

        public AnswerSlackActionHandlerTests()
        {
            _questionServiceMock = new Mock<IQuestionService>();
            _slackClientMock = new Mock<ISlackHttpClient>();
            var logger = new Mock<ILogger<AnswerSlackActionHandler>>();
            _handler = new AnswerSlackActionHandler(_slackClientMock.Object, _questionServiceMock.Object,
                logger.Object);
        }

        [Fact]
        public async Task Handle_ParamsAreNull_ArgumentNullException()
        {
            // Act-Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>  _handler.Handle(null));
        }

        [Fact]
        public async Task Handle_UserInParamsIsNull_ArgumentNullException()
        {
            // Arrange
            var actionParams = new AnswerSlackActionParams
            {
                ButtonParams = new AnswerActionButtonParams()
            };

            // Act-Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(actionParams));
        }

        [Fact]
        public async Task Handle_ButtonParamsInParamsAreNull_ArgumentNullException()
        {
            // Arrange
            var actionParams = new AnswerSlackActionParams
            {
                User = new ItemInfo()
            };

            // Act-Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(actionParams));
        }

        [Fact]
        public async Task Handle_QuestionIdIsInvalid_ArgumentException()
        {
            // Arrange
            var actionParams = new AnswerSlackActionParams
            {
                User = new ItemInfo { Id = "userId", Name = "userName" },
                ButtonParams = new AnswerActionButtonParams { QuestionId = string.Empty }
            };

            // Act-Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(actionParams));
        }

        [Fact]
        public async Task Handle_QuestionNotFound_ArgumentNullException()
        {
            // Arrange
            const string questionId = "questionId";
            var actionParams = new AnswerSlackActionParams
            {
                User = new ItemInfo { Id = "userId", Name = "userName" },
                ButtonParams = new AnswerActionButtonParams { QuestionId = questionId }
            };

            _questionServiceMock.Setup(m => m.GetQuestionAsync(It.IsAny<string>()))
                .ReturnsAsync(default(Question));

            // Act-Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(actionParams));
        }

        [Fact]
        public async Task Handle_QuestionWithoutAnswers_ShouldSendMessageWithOneAttachment()
        {
            // Arrange
            var questionId = Guid.NewGuid();
            var actionParams = new AnswerSlackActionParams
            {
                User = new ItemInfo { Id = "userId", Name = "userName" },
                ButtonParams = new AnswerActionButtonParams { QuestionId = questionId.ToString() }
            };

            var question = new Question
            {
                Id = questionId,
                Text = "blabla"
            };

            var channel = new ChannelDto {Id = "channelId"};
            string actualChannelId = null;
            List<AttachmentDto> actualAttachments = null;

            _questionServiceMock.Setup(m => m.GetQuestionAsync(It.IsAny<string>()))
                .ReturnsAsync(question);
            _slackClientMock.Setup(m => m.OpenDirectMessageChannelAsync(It.IsAny<string>()))
                .ReturnsAsync(channel);
            _slackClientMock.Setup(m =>
                m.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<AttachmentDto>>()))
                .Returns(Task.CompletedTask)
                .Callback((string channelId, string message, List<AttachmentDto> attachments) =>
                {
                    actualChannelId = channelId;
                    actualAttachments = attachments;
                });

            // Act
            await _handler.Handle(actionParams);

            // Assert
            Assert.Equal(actualChannelId, channel.Id);
            Assert.Single(actualAttachments);
            _questionServiceMock.Verify(m => m.GetQuestionAsync(It.Is<string>(q => q == questionId.ToString())),
                Times.Once);
            _questionServiceMock.VerifyNoOtherCalls();
            _slackClientMock.Verify(
                m => m.OpenDirectMessageChannelAsync(It.Is<string>(id => id == actionParams.User.Id)), Times.Once);
            _slackClientMock.Verify(
                m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<AttachmentDto>>()),
                Times.Once);
            _slackClientMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_QuestionWithAnswers_ShouldSendMessageWithOnePlusNumberOfFoundQuestionsAttachments()
        {
            // Arrange
            const int defaultNumberOfAtachments = 1;
            var questionId = Guid.NewGuid();
            var actionParams = new AnswerSlackActionParams
            {
                User = new ItemInfo { Id = "userId", Name = "userName" },
                ButtonParams = new AnswerActionButtonParams { QuestionId = questionId.ToString() }
            };

            var answers = new List<Answer>
            {
                new Answer{Id = Guid.NewGuid(), Text = "answer 1 text"},
                new Answer{Id = Guid.NewGuid(), Text = "answer 2 text"}
            };

            var question = new Question
            {
                Id = questionId,
                Text = "blabla",
                Answers = answers
            };

            var channel = new ChannelDto { Id = "channelId" };
            string actualChannelId = null;
            List<AttachmentDto> actualAttachments = null;

            _questionServiceMock.Setup(m => m.GetQuestionAsync(It.IsAny<string>()))
                .ReturnsAsync(question);
            _slackClientMock.Setup(m => m.OpenDirectMessageChannelAsync(It.IsAny<string>()))
                .ReturnsAsync(channel);
            _slackClientMock.Setup(m =>
                m.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<AttachmentDto>>()))
                .Returns(Task.CompletedTask)
                .Callback((string channelId, string message, List<AttachmentDto> attachments) =>
                {
                    actualChannelId = channelId;
                    actualAttachments = attachments;
                });

            // Act
            await _handler.Handle(actionParams);

            // Assert
            Assert.Equal(actualChannelId, channel.Id);
            Assert.Equal(actualAttachments.Count, defaultNumberOfAtachments + answers.Count);
            _questionServiceMock.Verify(m => m.GetQuestionAsync(It.Is<string>(q => q == questionId.ToString())),
                Times.Once);
            _questionServiceMock.VerifyNoOtherCalls();
            _slackClientMock.Verify(
                m => m.OpenDirectMessageChannelAsync(It.Is<string>(id => id == actionParams.User.Id)), Times.Once);
            _slackClientMock.Verify(
                m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<AttachmentDto>>()),
                Times.Once);
            _slackClientMock.VerifyNoOtherCalls();
        }
    }
}
