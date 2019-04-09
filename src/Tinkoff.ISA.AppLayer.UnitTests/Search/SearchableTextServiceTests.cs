using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Moq;
using Tinkoff.ISA.AppLayer.Search;
using Tinkoff.ISA.DAL.Storage.Dao.Questions;
using Tinkoff.ISA.Domain;
using Tinkoff.ISA.Domain.Search;
using Xunit;

namespace Tinkoff.ISA.AppLayer.UnitTests.Search
{
    public class SearchableTextServiceTests
    {
        private readonly ISearchableTextService _service;
        private readonly Mock<IQuestionDao> _questionDaoMock;

        public SearchableTextServiceTests()
        {
            _questionDaoMock = new Mock<IQuestionDao>();
            _service = new SearchableTextService(_questionDaoMock.Object);
        }

        [Fact]
        public async Task GetQuestionsIdsFilter_NullCollection_ThrowsException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.GetQuestionsAsync(null));
        }

        [Fact]
        public async Task GetQuestionsDateFilter_JustInvoked_ShouldInvokeAppropriateDaoMethod()
        {
            // Arrange
            _questionDaoMock.Setup(m =>
                m.FindWithProjectionAndFilterAsync(It.IsAny<ProjectionDefinition<Question, SearchableQuestion>>(),
                    It.IsAny<FilterDefinition<Question>>()))
                .ReturnsAsync(new List<SearchableQuestion>());

            // Act
            await _service.GetQuestionsWithAnswersAsync(DateTime.UtcNow);

            // Assert
            _questionDaoMock.Verify(m => m.FindWithProjectionAndFilterAsync(It.IsAny<ProjectionDefinition<Question, SearchableQuestion>>(),
                It.IsAny<FilterDefinition<Question>>()), Times.Once);
            _questionDaoMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetQuestionsIdsFilter_JustInvoked_ShouldInvokeApproperiateDaoMethod()
        {
            // Arrange
            _questionDaoMock.Setup(m =>
                    m.FindWithProjectionAndFilterAsync(It.IsAny<ProjectionDefinition<Question, SearchableQuestion>>(),
                        It.IsAny<FilterDefinition<Question>>()))
                .ReturnsAsync(new List<SearchableQuestion>());

            // Act
            await _service.GetQuestionsWithAnswersAsync(DateTime.UtcNow);

            // Assert
            _questionDaoMock.Verify(m => m.FindWithProjectionAndFilterAsync(It.IsAny<ProjectionDefinition<Question, SearchableQuestion>>(),
                It.IsAny<FilterDefinition<Question>>()), Times.Once);
            _questionDaoMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetAnswersDateFilter_JustInvoked_ShouldReturnWithLaterDate()
        {
            // Arrange
            var freshIntervalMin = 15;
            var currentDate = DateTime.UtcNow;
            var boundDate = currentDate.Subtract(TimeSpan.FromMinutes(freshIntervalMin + 1));

            var onBoundAnswer = new Answer
            {
                Id = Guid.NewGuid(),
                LastUpdate = boundDate
            };

            var freshAnswer = new Answer
            {
                Id = Guid.NewGuid(),
                LastUpdate = currentDate
            };

            var questions = new List<Question>
            {
                new Question
                {
                    Answers = new List<Answer> {freshAnswer, onBoundAnswer}
                }
            };

            _questionDaoMock.Setup(m =>
                    m.FindWithProjectionAndFilterAsync(It.IsAny<ProjectionDefinition<Question, Question>>(),
                        It.IsAny<FilterDefinition<Question>>()))
                .ReturnsAsync(questions);

            // Act
            var searchableAnswers = await _service.GetAnswersAsync(boundDate);

            // Assert
            var searchableAnswersList = searchableAnswers.ToList();
            Assert.NotEmpty(searchableAnswersList);
            Assert.DoesNotContain(searchableAnswersList, a => a.Id == onBoundAnswer.Id.ToString());
            _questionDaoMock.Verify(m => m.FindWithProjectionAndFilterAsync(It.IsAny<ProjectionDefinition<Question, Question>>(),
                It.IsAny<FilterDefinition<Question>>()), Times.Once);
            _questionDaoMock.VerifyNoOtherCalls();
        }
    }
}
