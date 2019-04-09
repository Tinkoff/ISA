using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Tinkoff.ISA.AppLayer.Questions;
using Tinkoff.ISA.AppLayer.Slack.Common;
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
    public class ShowAnswersSlackActionHandlerTests
    {
        private readonly Mock<IQuestionService> _questionServiceMock;
        private readonly Mock<ISlackHttpClient> _slackClientMock;
        private readonly ShowAnswersSlackActionHandler _handler;

        public ShowAnswersSlackActionHandlerTests()
        {
            _questionServiceMock = new Mock<IQuestionService>();
            _slackClientMock = new Mock<ISlackHttpClient>();
            var logger = new Mock<ILogger<ShowAnswersSlackActionHandler>>();
            _handler = new ShowAnswersSlackActionHandler(_questionServiceMock.Object, _slackClientMock.Object,
                logger.Object);
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
            await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(new ShowAnswersSlackActionParams()));
        }

        [Fact]
        public async Task Handle_CorrectParams_ShouldShowAnswersToTheChannel()
        {
            //arrange
            var answer = new Answer {Id = Guid.NewGuid(), Rank = 10, Text = "text"};

            var question = new Question
            {
                Id = Guid.NewGuid(),
                Answers = new List<Answer>
                {
                    answer
                },
                LastUpdate = new DateTime(11, 1, 1),
                Text = "text"
            };

            var attachment = new AttachmentDto
            {
                Text = "questionText",
                Actions = new List<AttachmentActionDto> { new AttachmentActionDto("test", "test") },
            };

            var originalMessage = new OriginalMessageDto
            {
                Text = "testText",
                Attachments = new List<AttachmentDto> { attachment },
                TimeStamp = "time"
            };

            var actionParams = new ShowAnswersSlackActionParams()
            {
                User = new ItemInfo
                {
                    Id = "id",
                    Name = "Bob"
                },
                AttachmentId = 0,
                OriginalMessage = originalMessage,
                Channel = new ItemInfo
                {
                    Id = "testChannel"
                },
                ButtonParams = new ShowAnswersActionButtonParams
                {
                    QuestionId = "id"
                }
                
            };

            _questionServiceMock.Setup(s => s.GetQuestionAsync(actionParams.ButtonParams.QuestionId))
                .ReturnsAsync(question);

            _slackClientMock.Setup(s => s.UpdateMessageAsync(actionParams.OriginalMessage.TimeStamp,
                actionParams.Channel.Id, actionParams.OriginalMessage.Text,
                actionParams.OriginalMessage.Attachments))
                .Returns(Task.CompletedTask);

            //act 
            await _handler.Handle(actionParams);

            //assert
            _questionServiceMock.Verify(
                v => v.GetQuestionAsync(actionParams.ButtonParams.QuestionId), Times.Once);
            _questionServiceMock.VerifyNoOtherCalls();
            _slackClientMock.Verify(
                v => v.UpdateMessageAsync(actionParams.OriginalMessage.TimeStamp,
                    actionParams.Channel.Id, actionParams.OriginalMessage.Text,
                    actionParams.OriginalMessage.Attachments), Times.Once);
            _slackClientMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_CorrectParams_ShouldFindBestAnswerAndSendItWithInfo()
        {
            //arange
            var bestAnswer = new Answer { Id = Guid.NewGuid(), Rank = 10, Text = "text" };
            var answer1 = new Answer { Id = Guid.NewGuid(), Rank = 1, Text = "text" };
            var answer2 = new Answer { Id = Guid.NewGuid(), Rank = -20, Text = "text" };

            var question = new Question
            {
                Id = Guid.NewGuid(),
                Answers = new List<Answer>
                {   
                    answer1, bestAnswer, answer2
                },
                LastUpdate = new DateTime(11, 1, 1),
                Text = "text"
            };

            var sourceText = "questionText";

            var attachment = new AttachmentDto
            {
                Text = sourceText,
                Actions = new List<AttachmentActionDto> { new AttachmentActionDto("test", "test") },
            };

            var originalMessage = new OriginalMessageDto
            {
                Text = "testText",
                Attachments = new List<AttachmentDto> { attachment },
                TimeStamp = "time"
            };

            var actionParams = new ShowAnswersSlackActionParams()
            {
                User = new ItemInfo
                {
                    Id = "id",
                    Name = "Bob"
                },
                AttachmentId = 0,
                OriginalMessage = originalMessage,
                Channel = new ItemInfo
                {
                    Id = "testChannel"
                },
                ButtonParams = new ShowAnswersActionButtonParams
                {
                    QuestionId = "id"
                }
            };

            _questionServiceMock.Setup(s => s.GetQuestionAsync(actionParams.ButtonParams.QuestionId))
                .ReturnsAsync(question);
            _slackClientMock.Setup(s => s.UpdateMessageAsync(actionParams.OriginalMessage.TimeStamp,
                    actionParams.Channel.Id, actionParams.OriginalMessage.Text,
                    actionParams.OriginalMessage.Attachments))
                .Returns(Task.CompletedTask);

            //act
            await _handler.Handle(actionParams);

            //assert
            Assert.NotEqual(sourceText, actionParams.OriginalMessage.Attachments[actionParams.AttachmentId].Text);
            _slackClientMock.Verify(m => m.UpdateMessageAsync(actionParams.OriginalMessage.TimeStamp, actionParams.Channel.Id, actionParams.OriginalMessage.Text,
                actionParams.OriginalMessage.Attachments), Times.Once);
            _slackClientMock.VerifyNoOtherCalls();


        }
    }
}
