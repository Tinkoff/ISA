using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Tinkoff.ISA.AppLayer.Slack.Common;
using Tinkoff.ISA.AppLayer.Slack.Event.ButtonParams;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.ActionHandlers;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.ActionHandlers.Params;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.Request;
using Tinkoff.ISA.DAL.Slack;
using Tinkoff.ISA.DAL.Slack.Dtos;
using Xunit;

namespace Tinkoff.ISA.AppLayer.UnitTests.Slack.ActionHandlers
{
    public class AddAnswerSlackActionHandlerTests
    {
        private readonly Mock<ISlackHttpClient> _slackHttpClientMock;
        private readonly Mock<ICallbackIdCustomParamsWrappingService> _callbackIdCustomParamsWrapperMock;
        private readonly AddAnswerSlackActionHandler _handler;

        public AddAnswerSlackActionHandlerTests()
        {
            _slackHttpClientMock = new Mock<ISlackHttpClient>();
            _callbackIdCustomParamsWrapperMock = new Mock<ICallbackIdCustomParamsWrappingService>();
            var logger = new Mock<ILogger<AddAnswerSlackActionHandler>>();
            _handler = new AddAnswerSlackActionHandler(_slackHttpClientMock.Object, logger.Object,
                _callbackIdCustomParamsWrapperMock.Object);
        }

        [Fact]
        public async Task Handle_ParamsAreNull_ArgumentNullException()
        {
            // Act-Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null));
        }

        [Fact]
        public async Task Handle_UserInParamsIsNull_ArgumentNullException()
        {
            // Arrange
            var actionParams = new AddAnswerSlackActionParams
            {
                ButtonParams = new AddAnswerActionButtonParams(),
                TriggerId = "triggerId"
            };

            // Act-Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(actionParams));
        }

        [Fact]
        public async Task Handle_ButtonParamsInParamsAreNull_ArgumentNullException()
        {
            // Arrange
            var actionParams = new AddAnswerSlackActionParams
            {
                User = new ItemInfo(),
                TriggerId = "triggerId"
            };

            // Act-Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(actionParams));
        }

        [Fact]
        public async Task Handle_QuestionIdIsInvalid_ArgumentException()
        {
            // Arrange
            var actionParams = new AddAnswerSlackActionParams
            {
                User = new ItemInfo { Id = "userId", Name = "userName" },
                ButtonParams = new AddAnswerActionButtonParams { QuestionId = string.Empty },
                TriggerId = "triggerId"
            };

            // Act-Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(actionParams));
        }

        [Fact]
        public async Task Handle_TriggerIdIsInvalid_ArgumentException()
        {
            // Arrange
            var actionParams = new AddAnswerSlackActionParams
            {
                User = new ItemInfo { Id = "userId", Name = "userName" },
                ButtonParams = new AddAnswerActionButtonParams { QuestionId = "questionId" },
                TriggerId = string.Empty
            };

            // Act-Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(actionParams));
        }

        [Fact]
        public async Task Handle_CorrectParams_ShouldOpenAppropriateDialog()
        {
            // Arrange
            var actionParams = new AddAnswerSlackActionParams
            {
                User = new ItemInfo { Id = "userId", Name = "userName" },
                ButtonParams = new AddAnswerActionButtonParams { QuestionId = "questionId" },
                TriggerId = "triggerId",
                OriginalMessage = new OriginalMessageDto
                {
                    TimeStamp = "state"
                }
            };
            
            DialogRequest actualRequest = null;
            _slackHttpClientMock.Setup(m => m.OpenDialogAsync(It.IsAny<DialogRequest>()))
                .Returns(Task.CompletedTask)
                .Callback((DialogRequest request) => actualRequest = request);

            // Act
            await _handler.Handle(actionParams);

            // Assert
            Assert.Equal(actualRequest.TriggerId, actionParams.TriggerId);
            _slackHttpClientMock.Verify(m => m.OpenDialogAsync(It.IsAny<DialogRequest>()), Times.Once);
            _slackHttpClientMock.VerifyNoOtherCalls();
        }
    }
}
