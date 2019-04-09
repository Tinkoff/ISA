using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Tinkoff.ISA.AppLayer.Search;
using Tinkoff.ISA.AppLayer.Slack.Common;
using Tinkoff.ISA.AppLayer.Slack.Event;
using Tinkoff.ISA.AppLayer.Slack.Event.Request;
using Tinkoff.ISA.DAL.Elasticsearch.Request;
using Tinkoff.ISA.DAL.Elasticsearch.Services;
using Tinkoff.ISA.DAL.Slack;
using Tinkoff.ISA.DAL.Slack.Dtos;
using Tinkoff.ISA.Domain.Search;
using Tinkoff.ISA.Infrastructure.Settings;
using Xunit;

namespace Tinkoff.ISA.AppLayer.UnitTests.Slack
{
    public class EventServiceTests
    {
        private readonly Mock<ISlackHttpClient> _slackClientMock;
        private readonly Mock<IElasticSearchService> _elasticSearchService;
        private readonly Mock<ISearchableTextService> _searchableTextServiceMock;
        private readonly EventService _eventService;
        private readonly SearchableQuestion _question;
        private readonly SearchableAnswer _answer;
        private readonly EventWrapperRequest _request;

        public EventServiceTests()
        {
            var elasticsearchSettingsMock = new Mock<IOptions<ElasticsearchSettings>>();
            elasticsearchSettingsMock
                .SetupGet(m => m.Value)
                .Returns(() => new ElasticsearchSettings());

            _elasticSearchService = new Mock<IElasticSearchService>();
            _slackClientMock = new Mock<ISlackHttpClient>();
            _searchableTextServiceMock = new Mock<ISearchableTextService>();
            var logger = new Mock<ILogger<EventService>>();
            _eventService = new EventService(_slackClientMock.Object, _elasticSearchService.Object,
                _searchableTextServiceMock.Object, logger.Object);

            var slackEvent = new SlackEvent
            {
                Text = "Hello from Slack!",
                Channel = "CBGJMA0TA",
                UserId = "UBJ6GC75K",
                Type = "message"
            };
            _request = new EventWrapperRequest
            {
                Event = slackEvent
            };

            _question = new SearchableQuestion
            {
                Id = Guid.NewGuid().ToString(),
                Text = "???"
            };

            _answer = new SearchableAnswer()
            {
                Text = "???",
                Id = Guid.NewGuid().ToString(),
                QuestionId = Guid.NewGuid().ToString()
            };

        }

        [Fact]
        public async Task ProcessRequest_EventTypeWrong_Ignore()
        {
            //Arrange
            var slackEvent = new SlackEvent
            {
                Type = "EmojiReaction"
            };
            var request = new EventWrapperRequest
            {
                Event = slackEvent
            };

            // Act
            await _eventService.ProcessRequest(request);

            // Assert
            _elasticSearchService.VerifyNoOtherCalls();
            _slackClientMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ProcessRequest_MessageFromBot_Ignore()
        {
            //Arrange
            var slackEvent = new SlackEvent
            {
                BotId = "123456"
            };
            var request = new EventWrapperRequest
            {
                Event = slackEvent
            };

            // Act
            await _eventService.ProcessRequest(request);

            // Assert
            _elasticSearchService.VerifyNoOtherCalls();
            _slackClientMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ProcessRequest_MessageTextIsEmpty_Ignore()
        {
            //Arrange
            var slackEvent = new SlackEvent
            {
                Text = string.Empty
            };
            var request = new EventWrapperRequest
            {
                Event = slackEvent
            };

            // Act
            await _eventService.ProcessRequest(request);

            // Assert
            _elasticSearchService.VerifyNoOtherCalls();
            _slackClientMock.VerifyNoOtherCalls();
        }
        
        [Fact]
        public async Task ProcessRequest_NecessaryNumberOfSimilarQuestions_ShouldSendQuestionsLikeRequest()
        {
            // Arrange
            var questions = new List<SearchableQuestion>
            {   
                _question, _question, _question, _question, _question
            };

            _elasticSearchService.Setup(s => s
                    .SearchAsync<SearchableQuestion>(It.IsAny<ElasticSearchRequest>()))
                .ReturnsAsync(questions);

            _elasticSearchService.Setup(m =>
                    m.SearchWithTitleAsync<SearchableConfluence>(It.IsAny<ConfluenceElasticSearchRequest>()))
                .ReturnsAsync(new List<SearchableConfluence>());

            _elasticSearchService
                .Setup(m => m.SearchWithTitleAsync<SearchableJira>(It.IsAny<JiraElasticSearchRequest>()))
                .ReturnsAsync(new List<SearchableJira>());

            _slackClientMock
                .Setup(m => m.SendMessageAsync(_request.Event.Channel, Phrases.SimilarQuestions, It.IsAny<List<AttachmentDto>>()))
                .Returns(Task.CompletedTask);

            // Act
            await _eventService.ProcessRequest(_request);

            // Assert
            _elasticSearchService.Verify(
                m => m.SearchAsync<SearchableQuestion>(It.IsAny<ElasticSearchRequest>()), Times.Once);
            _elasticSearchService.Verify(
                m => m.SearchWithTitleAsync<SearchableConfluence>(It.IsAny<ConfluenceElasticSearchRequest>()),
                Times.Once);
            _elasticSearchService.Verify(
                m => m.SearchWithTitleAsync<SearchableJira>(It.IsAny<JiraElasticSearchRequest>()), Times.Once);
            _elasticSearchService.VerifyNoOtherCalls();
            _slackClientMock.Verify(m => m.SendMessageAsync(_request.Event.Channel, Phrases.SimilarQuestions, It.IsAny<List<AttachmentDto>>()));
            _slackClientMock.VerifyNoOtherCalls();
            
        }
        
        [Fact]
        public async Task ProcessRequest_TwoSimilarQuestions_ShoulSendQuestionsAndAnswersLikeRequest()
        {
            // Arrange
            var questions = new List<SearchableQuestion> {_question, _question};
            var answers = new List<SearchableAnswer> {_answer};
            var connectedQuestions = new List<SearchableQuestion>
            {
                new SearchableQuestion{Id = _answer.QuestionId, Text = "abc"}
            };

            _elasticSearchService.Setup(s => s
                    .SearchAsync<SearchableQuestion>(It.IsAny<ElasticSearchRequest>()))
                .ReturnsAsync(questions);

            _elasticSearchService.Setup(s => s
                    .SearchAsync<SearchableAnswer>(It.IsAny<ElasticSearchRequest>()))
                .ReturnsAsync(answers);

            _elasticSearchService.Setup(m =>
                    m.SearchWithTitleAsync<SearchableConfluence>(It.IsAny<ConfluenceElasticSearchRequest>()))
                .ReturnsAsync(new List<SearchableConfluence>());

            _elasticSearchService
                .Setup(m => m.SearchWithTitleAsync<SearchableJira>(It.IsAny<JiraElasticSearchRequest>()))
                .ReturnsAsync(new List<SearchableJira>());

            _searchableTextServiceMock.Setup(s => s.GetQuestionsAsync(It.IsAny<IEnumerable<Guid>>()))
                .ReturnsAsync(connectedQuestions);

            _slackClientMock
                .Setup(m => m.SendMessageAsync(_request.Event.Channel, Phrases.SimilarQuestions, It.IsAny<List<AttachmentDto>>()))
                .Returns(Task.CompletedTask);
            
            // Act
            await _eventService.ProcessRequest(_request);

            // Assert
            _elasticSearchService.Verify(
                m => m.SearchAsync<SearchableQuestion>(It.IsAny<ElasticSearchRequest>()), Times.Once);
            _elasticSearchService.Verify(
                m => m.SearchAsync<SearchableAnswer>(It.IsAny<ElasticSearchRequest>()), Times.Once);
            _elasticSearchService.Verify(
                m => m.SearchWithTitleAsync<SearchableConfluence>(It.IsAny<ConfluenceElasticSearchRequest>()),
                Times.Once);
            _elasticSearchService.Verify(
                m => m.SearchWithTitleAsync<SearchableJira>(It.IsAny<JiraElasticSearchRequest>()), Times.Once);
            _elasticSearchService.VerifyNoOtherCalls();
            _searchableTextServiceMock.Verify(s =>
                s.GetQuestionsAsync(It.IsAny<IEnumerable<Guid>>()));
            _elasticSearchService.VerifyNoOtherCalls();
            _slackClientMock.Verify(m => m.SendMessageAsync(_request.Event.Channel, Phrases.SimilarQuestions, It.IsAny<List<AttachmentDto>>()));
            _slackClientMock.VerifyNoOtherCalls();

        }

        [Fact]
        public async Task ProcessRequest_AnyExceptionOccured_ShouldNotifySlackUser()
        {
            // Arrange
            _elasticSearchService.Setup(m => m.SearchAsync<SearchableQuestion>(It.IsAny<ElasticSearchRequest>()))
                .ThrowsAsync(new ArgumentNullException());

            _slackClientMock.Setup(m =>
                    m.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<AttachmentDto>>()))
                .Returns(Task.CompletedTask);

            // Act
            await _eventService.ProcessRequest(_request);

            // Assert
            _elasticSearchService.Verify(m => m.SearchAsync<SearchableQuestion>(It.IsAny<ElasticSearchRequest>()),
                 Times.Once);
            _elasticSearchService.VerifyNoOtherCalls();
            _slackClientMock.Verify(m =>
                m.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<AttachmentDto>>()),
                Times.Once);
            _slackClientMock.VerifyNoOtherCalls();
        }
    }
}