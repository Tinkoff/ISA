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
    public class StopNotificationsSlackActionHandlerTests
    {
        private readonly Mock<IQuestionService> _questionServiceMock;
        private readonly Mock<ISlackHttpClient> _slackClientMock;
        private readonly StopNotifyUserSlackActionHandler _handler;

        private const string QuestionId = "questionId";
        private const string UserId = "userId";
        private const string ChannelId = "chanelId";


        public StopNotificationsSlackActionHandlerTests()
        {
            _questionServiceMock = new Mock<IQuestionService>();
            _slackClientMock = new Mock<ISlackHttpClient>();
            _handler = new StopNotifyUserSlackActionHandler(_slackClientMock.Object,
                _questionServiceMock.Object,
                new Mock<ILogger<StopNotifyUserSlackActionHandler>>().Object);
        }
        
        
        [Fact]
        public async Task Handle_ParamsAreNull_ArgumentNullException()
        {
            // Act, Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null));
        }
        
        [Fact]
        public async Task Handle_ButtonParamsAreNull_ArgumentNullException()
        {
            // Act-Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(new StopNotificationsSlackActionParams()));
        }

        [Fact]
        public async Task Handle_CorrectParams_ShouldStopNotificationForUser()
        {            
            //act
            await _handler.Handle(CreateParams());
            
            //assert
            _questionServiceMock.Verify(v => v.UnsubscribeNotificationForUser(QuestionId, UserId), Times.Once);
        }

        [Fact]
        public async Task Handle_CorrectParams_ShouldUpdateMessage()
        {
            //act
            await _handler.Handle(CreateParams());
            
            _slackClientMock.Verify(v => v.UpdateMessageAsync(It.IsAny<string>(), ChannelId, 
                It.IsAny<string>(), It.IsAny<List<AttachmentDto>>()), Times.Once);
        }


        private StopNotificationsSlackActionParams CreateParams()
        {
            return new StopNotificationsSlackActionParams
            {
                AttachmentId = 0,
                
                OriginalMessage = new OriginalMessageDto
                {
                    TimeStamp = "11",
                    Text = "text",
                    Attachments = new List<AttachmentDto>
                    {
                        new AttachmentDto
                        {
                            Actions = new List<AttachmentActionDto>()
                        }
                    }
                },
                
                ButtonParams = new StopNotificationsActionButtonParams
                {
                    QuestionId = QuestionId
                },
                
                User = new ItemInfo
                {
                    Name = "alo",
                    Id = UserId
                },
                
                Channel = new ItemInfo
                {
                    Id = ChannelId
                }
            };
        }
    }
}