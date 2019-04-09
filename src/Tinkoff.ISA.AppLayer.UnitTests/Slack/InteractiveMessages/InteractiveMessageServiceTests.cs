using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using Tinkoff.ISA.AppLayer.Slack.Executing;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.ActionHandlers.Params;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.Request;
using Tinkoff.ISA.AppLayer.Slack.TypeChooser.ActionParams;
using Tinkoff.ISA.DAL.Slack.Dtos;
using Xunit;

namespace Tinkoff.ISA.AppLayer.UnitTests.Slack.InteractiveMessages
{
    public class InteractiveMessageServiceTests
    {
        private readonly Mock<ISlackParamsSelectService> _slackParamsSelectServiceMock;
        private readonly Mock<ISlackExecutorService> _slackExecutorServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly InteractiveMessageService _service;

        public InteractiveMessageServiceTests()
        {
            _slackParamsSelectServiceMock = new Mock<ISlackParamsSelectService>();
            _slackExecutorServiceMock = new Mock<ISlackExecutorService>();
            _mapperMock = new Mock<IMapper>();
            _service = new InteractiveMessageService(_slackParamsSelectServiceMock.Object,
                _slackExecutorServiceMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task ProcessRequest_MessageIsNull_ArgumentNullException()
        {
            // Act-Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.ProcessRequest(null));
        }

        [Fact]
        public async Task ProcessRequest_NoActions_ArgumentNullException()
        {
            // Act-Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.ProcessRequest(new InteractiveMessage
            {
                Actions = new List<AttachmentActionDto>()
            }));
        }

        [Fact]
        public async Task ProccesRequest_HelpedAction_ShouldCallExecutorServiceWithAppropriateArgs()
        {
            // Arrange
            var action = new AttachmentActionDto("actionName", "some text");
            var request = new InteractiveMessage
            {
                Actions = new List<AttachmentActionDto> {action}
            };
            var paramsType = typeof(HelpedSlackActionParams);
            var actionParams = new HelpedSlackActionParams();

            _slackParamsSelectServiceMock.Setup(m => m.Choose(It.IsAny<string>()))
                .Returns(paramsType);   
            _mapperMock.Setup(m => m.Map(It.IsAny<object>(), It.IsAny<Type>(), It.IsAny<Type>()))
                .Returns(actionParams);
            _slackExecutorServiceMock.Setup(m => m.ExecuteAction(It.IsAny<Type>(), It.IsAny<object[]>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.ProcessRequest(request);

            // Assert
            _slackParamsSelectServiceMock.Verify(m => m.Choose(It.Is<string>(n => n == action.Name)), Times.Once);
            _slackParamsSelectServiceMock.VerifyNoOtherCalls();
            _mapperMock.Verify(m => m.Map(It.Is<object>(r => r == request), It.Is<Type>(t => t == request.GetType()),
                It.Is<Type>(t => t == actionParams.GetType())));
            _mapperMock.VerifyNoOtherCalls();
            _slackExecutorServiceMock.Verify(m => m.ExecuteAction(It.Is<Type>(t => t == paramsType), It.IsAny<object[]>()), Times.Once);
            _slackExecutorServiceMock.VerifyNoOtherCalls();
        }
    }
}
