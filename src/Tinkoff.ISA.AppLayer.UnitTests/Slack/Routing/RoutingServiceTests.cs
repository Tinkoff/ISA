using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.ObjectModel;
using Moq;
using Newtonsoft.Json;
using Tinkoff.ISA.AppLayer.Slack.Dialogs.Submissions;
using Tinkoff.ISA.AppLayer.Slack.Executing;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.Request;
using Tinkoff.ISA.AppLayer.Slack.Routing;
using Tinkoff.ISA.AppLayer.Slack.TypeChooser.DialogSubmission;
using Tinkoff.ISA.DAL.Slack;
using Tinkoff.ISA.Infrastructure.Settings;
using Xunit;
using MessageType = Tinkoff.ISA.AppLayer.Slack.Common.MessageType;

namespace Tinkoff.ISA.AppLayer.UnitTests.Slack.Routing
{
    public class RoutingServiceTests
    {
        private readonly Mock<IInteractiveMessageService> _interactiveMessageServiceMock;
        private readonly Mock<ISubmissionSelectService> _submissionSelectServiceMock;
        private readonly Mock<ISlackExecutorService> _slackExecutorServiceMock;
        private readonly Mock<ISlackHttpClient> _slackHttpClientMock;
        private readonly RoutingService _service;

        public RoutingServiceTests()
        {
            _interactiveMessageServiceMock = new Mock<IInteractiveMessageService>();
            _submissionSelectServiceMock = new Mock<ISubmissionSelectService>();
            _slackExecutorServiceMock = new Mock<ISlackExecutorService>();
            _slackHttpClientMock = new Mock<ISlackHttpClient>();
            var logger = new Mock<ILogger<RoutingService>>();
            _service = new RoutingService(_interactiveMessageServiceMock.Object, _submissionSelectServiceMock.Object,
                _slackExecutorServiceMock.Object, _slackHttpClientMock.Object, logger.Object);
        }

        [Fact]
        public async Task Route_PayloadIsEmpty_ArgumentException()
        {
            // Act-Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.Route(string.Empty));
        }

        [Fact]
        public async Task Route_InvocationIsInteractiveMessage_ShouldCallInteractiveMessageService()
        {
            // Arrange
            var message = new InteractiveMessage
            {
                Type = MessageType.InteractiveMessage
            };

            var payloadRaw = JsonConvert.SerializeObject(message, SlackSerializerSettings.DefaultSettings);

            _interactiveMessageServiceMock.Setup(m => m.ProcessRequest(It.IsAny<InteractiveMessage>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.Route(payloadRaw);
            
            // Assert
            _interactiveMessageServiceMock.Verify(m => m.ProcessRequest(It.IsAny<InteractiveMessage>()), Times.Once);
            _interactiveMessageServiceMock.VerifyNoOtherCalls();
            _slackExecutorServiceMock.VerifyNoOtherCalls();
            _submissionSelectServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Route_InvocationIsAddAnswerDialogSubmission_ShouldCallAppropriateServices()
        {
            // Arrange
            var dialog = new DialogSubmission<AddAnswerSubmission>
            {
                Type = MessageType.DialogSubmission,
                CallbackId = "callbackId"
            };

            var payloadRaw = JsonConvert.SerializeObject(dialog, SlackSerializerSettings.DefaultSettings);

            _submissionSelectServiceMock.Setup(m => m.Choose(It.IsAny<string>()))
                .Returns(typeof(DialogSubmission<AddAnswerSubmission>));
            _slackExecutorServiceMock.Setup(m => m.ExecuteSubmission(It.IsAny<Type>(), It.IsAny<object[]>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.Route(payloadRaw);

            // Assert
            _submissionSelectServiceMock.Verify(m => m.Choose(It.Is<string>(c => c == dialog.CallbackId)), Times.Once);
            _submissionSelectServiceMock.VerifyNoOtherCalls();
            _slackExecutorServiceMock.Verify(
                m => m.ExecuteSubmission(It.Is<Type>(t => t == typeof(DialogSubmission<AddAnswerSubmission>)), It.IsAny<object[]>()),
                Times.Once);
            _slackExecutorServiceMock.VerifyNoOtherCalls();
            _interactiveMessageServiceMock.VerifyNoOtherCalls();
        }
    }
}
