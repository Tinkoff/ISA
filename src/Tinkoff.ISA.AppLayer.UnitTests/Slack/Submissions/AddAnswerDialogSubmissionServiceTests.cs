using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Tinkoff.ISA.Domain;
using Tinkoff.ISA.AppLayer.Questions;
using Tinkoff.ISA.AppLayer.Slack.Common;
using Tinkoff.ISA.AppLayer.Slack.Dialogs;
using Tinkoff.ISA.AppLayer.Slack.Dialogs.Submissions;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.Request;
using Tinkoff.ISA.DAL.Slack;
using Tinkoff.ISA.DAL.Slack.Dtos;
using Xunit;

namespace Tinkoff.ISA.AppLayer.UnitTests.Slack.Submissions
{
    public class AddAnswerDialogSubmissionServiceTests
    {
        private readonly Mock<IQuestionService> _questionServiceMock;
        private readonly Mock<ISlackHttpClient> _slackHttpClientMock;
        private readonly Mock<ICallbackIdCustomParamsWrappingService> _callbackIdCustomParamsWrapperMock;
        private readonly AddAnswerDialogSubmissionService _service;

        public AddAnswerDialogSubmissionServiceTests()
        {
            _questionServiceMock = new Mock<IQuestionService>();
            _slackHttpClientMock = new Mock<ISlackHttpClient>();
            _callbackIdCustomParamsWrapperMock = new Mock<ICallbackIdCustomParamsWrappingService>();
            var logger = new Mock<ILogger<AddAnswerDialogSubmissionService>>();
            _service = new AddAnswerDialogSubmissionService(_questionServiceMock.Object, _slackHttpClientMock.Object,
                logger.Object, _callbackIdCustomParamsWrapperMock.Object);
        }

        [Fact]
        public async Task ProcessSubmission_SubmissionIsNull_ArgumentNullException()
        {
            // Act-Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.ProcessSubmission(null));
        }

        [Fact]
        public async Task ProcessSubmission_CallbackIdDoesNotContainQuestionId_ArgumentNullException()
        {
            // Arrange
            const string expectedQuestionId = "questionId";
            var dialog = new DialogSubmission<AddAnswerSubmission>
            {
                User = new ItemInfo
                {
                    Id = "userId",
                    Name = "userName"
                },
                Channel = new ItemInfo
                {
                    Id = "channelId",
                    Name = "ChannelName"
                },
                CallbackId = expectedQuestionId,
                Submission = new AddAnswerSubmission
                {
                    ExpertsAnswer = "Answer goes here"
                }
            };

            _callbackIdCustomParamsWrapperMock.Setup(m => m.Unwrap(It.IsAny<string>()))
                .Returns(new List<string>());

            // Act-Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.ProcessSubmission(dialog));
        }

        [Fact]
        public async Task ProcessSubmission_JustInvoked_ShouldSendMessageToChannel()
        {
            // Arrange
            const string expectedQuestionId = "questionId";
            var dialog = new DialogSubmission<AddAnswerSubmission>
            {
                User = new ItemInfo
                {
                    Id = "userId",
                    Name = "userName"
                },
                Channel = new ItemInfo
                {
                    Id = "channelId",
                    Name = "ChannelName"
                },
                CallbackId = expectedQuestionId,
                Submission = new AddAnswerSubmission
                {
                    ExpertsAnswer = "Answer goes here"
                }
            };

            var question = new Question
            {
                Text = "text",
                AskedUsersIds = new List<string>()
            };

            string actualChannelId = null;
            string actualQuestionId = null;
            string actualAppendedAnswer = null;

            _callbackIdCustomParamsWrapperMock.Setup(m => m.Unwrap(It.IsAny<string>()))
                .Returns(new List<string>{ expectedQuestionId });
            _questionServiceMock.Setup(m => m.AppendAnswerAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask)
                .Callback((string questionId, string answer) =>
                {
                    actualQuestionId = questionId;
                    actualAppendedAnswer = answer;
                });
            _questionServiceMock.Setup(s => s.GetQuestionAsync(It.IsAny<string>())).ReturnsAsync(question);
            
            _slackHttpClientMock.Setup(m =>
                    m.UpdateMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<AttachmentDto>>()))
                .Returns(Task.CompletedTask)
                .Callback((string timeStamp,string channelId, string message, List<AttachmentDto> attachments) => actualChannelId = channelId);

            // Act
            await _service.ProcessSubmission(dialog);

            // Assert
            Assert.Equal(actualChannelId, dialog.Channel.Id);
            Assert.Equal(actualAppendedAnswer, dialog.Submission.ExpertsAnswer);
            Assert.Equal(actualQuestionId, expectedQuestionId);
            _questionServiceMock.Verify(m => m.AppendAnswerAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _questionServiceMock.Verify(s => s.GetQuestionAsync(It.IsAny<string>()), Times.Once);
            _questionServiceMock.VerifyNoOtherCalls();
            _slackHttpClientMock.Verify(
                m => m.UpdateMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<AttachmentDto>>()),
                Times.Once);
            _slackHttpClientMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ProcessSubmission_JustInvoked_ShouldSendNotificationForAllSubscribers()
        {
            //Arrange
            var users = new List<string>
            {
                "1",
                "2"
            };

            var channelIds = new List<ChannelDto>
            {
                new ChannelDto
                {
                    Id = "channel1"
                },
                new ChannelDto
                {
                    Id = "channel2"
                }
            };
            
            
            var dialog = new DialogSubmission<AddAnswerSubmission>
            {
                User = new ItemInfo
                {
                    Id = "userId",    
                    Name = "userName"
                },
                Channel = new ItemInfo
                {
                    Id = "channelId",
                    Name = "ChannelName"
                },
                CallbackId = "questionId",
                Submission = new AddAnswerSubmission
                {
                    ExpertsAnswer = "Answer goes here"
                }
            };

            var question = new Question
            {
                Text = "text",
                AskedUsersIds = users
            };
            
            _callbackIdCustomParamsWrapperMock.Setup(m => m.Unwrap(It.IsAny<string>()))
                .Returns(new List<string>{ "Id" });
            
            _questionServiceMock
                .Setup(s => s.GetQuestionAsync(It.IsAny<string>()))
                .ReturnsAsync(question);

            _slackHttpClientMock
                .Setup(s => s.OpenDirectMessageChannelAsync(users[0]))
                .ReturnsAsync(channelIds[0]);
            _slackHttpClientMock
                .Setup(s => s.OpenDirectMessageChannelAsync(users[1]))
                .ReturnsAsync(channelIds[1]);
            
            // Act
            await _service.ProcessSubmission(dialog);
            
            //Assert
            _questionServiceMock.Verify(s => s.GetQuestionAsync(It.IsAny<string>()), Times.Once);
            
            
            _slackHttpClientMock
                .Verify(s => s.OpenDirectMessageChannelAsync(It.IsAny<string>()), Times.Exactly(2));
           
            _slackHttpClientMock
                .Verify(s => s.SendMessageAsync(channelIds[0].Id, It.IsAny<string>(), It.IsAny<List<AttachmentDto>>()), Times.Once);
            _slackHttpClientMock
                .Verify(s => s.SendMessageAsync(channelIds[1].Id, It.IsAny<string>(), It.IsAny<List<AttachmentDto>>()), Times.Once);
        }
    }
}
