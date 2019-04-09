using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Moq;
using Tinkoff.ISA.AppLayer.Questions;
using Tinkoff.ISA.DAL.Storage.Common;
using Tinkoff.ISA.DAL.Storage.Dao.Questions;
using Tinkoff.ISA.Domain;
using Xunit;

namespace Tinkoff.ISA.AppLayer.UnitTests.Questions
{
    public class QuestionServiceTests
    {
        private const string UserId = "UBJ6GC75K";
        private const string QuestionText = "Who are you?";
        private const string QuestionId = "1";
        private const string AnswerId = "2";
        private readonly Mock<IRepository<Question, Guid>> _questionRepositoryMock;
        private readonly Mock<IQuestionDao> _questionDaoMock;
        private readonly IQuestionService _service;

        public QuestionServiceTests()
        {
            _questionRepositoryMock = new Mock<IRepository<Question, Guid>>();
            _questionDaoMock = new Mock<IQuestionDao>();
            _service = new QuestionService(_questionRepositoryMock.Object, _questionDaoMock.Object);
            
        }

        [Fact]
        public async Task ProcessQuestion_QuestionIsNull_ArgumentException()
        {
            // Act, assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.UpsertAsync(null));
        }

        [Fact]
        public async Task ProcessQuestion_JustInvoked_ExpectedQuestion()
        {
            // Arrange
            var question = new Question { Text = QuestionText };
            
            _questionDaoMock
                .Setup(m => m.UpsertAsync(It.IsAny<Expression<Func<Question, bool>>>(),
                        It.IsAny<Question>()))
                .ReturnsAsync(question);
            
            // Act
            var result = await _service.UpsertAsync(question);

            // Assert
            _questionDaoMock.Verify(m => m.UpsertAsync(It.IsAny<Expression<Func<Question, bool>>>(),
                It.IsAny<Question>()), Times.Once);
            _questionDaoMock.VerifyNoOtherCalls();
            Assert.Same(question, result);
        }

        [Fact]
        public async Task GetQuestion_InvalidUserId_ArgumentException()
        {
            // Act, assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetQuestionAsync(string.Empty));
        }

        [Fact]
        public async Task GetQuestion_JustInvoked_ExpectedQuestion()
        {
            // Arrange
            var question = new Question
            {
                Text = QuestionText
            };

            _questionRepositoryMock
                .Setup(m => m.FirstOrDefault(It.IsAny<Expression<Func<Question, bool>>>()))
                .ReturnsAsync(question);

            // Act
            var result = await _service.GetQuestionAsync(UserId);

            // Assert
            _questionRepositoryMock.Verify(m => m.FirstOrDefault(
                It.IsAny<Expression<Func<Question, bool>>>()),
                Times.Once);
            _questionRepositoryMock.VerifyNoOtherCalls();
            Assert.Same(result, question);
        }

        [Fact]
        public void RankUpAsync_CorrectParametres_ShouldCallUpdateRankAsync()
        {
            // Act
            _service.AnswerRankUpAsync(QuestionId, AnswerId);

            // Assert
            _questionDaoMock.Verify(
                m => m.UpdateRankAsync(It.Is<string>(x => x == QuestionId), It.Is<string>(x => x == AnswerId),
                    It.Is<int>(x => x == 1)),
                Times.Once());
        }

        [Fact]
        public void RankDownAsync_CorrectParametres_ShouldCallUpdateRankAsync()
        {
            // Act
            _service.AnswerRankDownAsync(QuestionId, AnswerId);

            // Assert
            _questionDaoMock.Verify(
                m => m.UpdateRankAsync(It.Is<string>(x => x == QuestionId), It.Is<string>(x => x == AnswerId),
                    It.Is<int>(x => x == -1)),
                Times.Once());
        }

        [Fact]
        public async void GetAnswersOnQuestionExceptAsync_ExceptInputAnswerCorrectId_ArrayWithoutInputAnswerId()
        {
            // Arrange
            var choosenAnswer = new Answer
            {
                Id = Guid.NewGuid(),
                Text = "choosen",
                Rank = 2
            };
            var otherAnswer1 = new Answer
            {
                Id = Guid.NewGuid(),
                Text = "other",
                Rank = 2
            };
            var otherAnswer2 = new Answer
            {
                Id = Guid.NewGuid(),
                Text = "other",
                Rank = 2
            };

            var foundedQuestion = new Question()
            {
                Id = Guid.NewGuid(),
                Text = "Вопрос",
                Answers = new[] { choosenAnswer, otherAnswer1, otherAnswer2 }
            };

            _questionRepositoryMock
                .Setup(m => m.FirstOrDefault(It.IsAny<Expression<Func<Question, bool>>>()))
                .ReturnsAsync(foundedQuestion);

            // Act 
            var result = await _service.GetAnswersOnQuestionExceptAsync(foundedQuestion.Id.ToString(),
                choosenAnswer.Id.ToString());

            // Assert
            Assert.Equal(result, new[] { otherAnswer1, otherAnswer2 });
        }

        [Fact]
        public async void GetAnswersOnQuestionExceptAsync_InCorrectAnswerId_OutputAnswersOrderedBydescedingOfRank()
        {
            // Arrange
            var answer1 = new Answer
            {
                Id = Guid.NewGuid(),
                Text = "choosen",
                Rank = 2
            };
            
            var answer2 = new Answer
            {
                Id = Guid.NewGuid(),
                Text = "other",
                Rank = 15
            };
            
            var answer3 = new Answer
            {
                Id = Guid.NewGuid(),
                Text = "other",
                Rank = -4
            };

            var foundedQuestion = new Question()
            {
                Id = Guid.NewGuid(),
                Text = "Вопрос",
                Answers = new[] { answer1, answer2, answer3 }
            };

            _questionRepositoryMock
                .Setup(m => m.FirstOrDefault(It.IsAny<Expression<Func<Question, bool>>>()))
                .ReturnsAsync(foundedQuestion);

            // Act 
            var result = await _service.GetAnswersOnQuestionExceptAsync(foundedQuestion.Id.ToString(),
                Guid.NewGuid().ToString());

            // Assert
            Assert.Equal(result, new[] { answer2, answer1, answer3 });
        }
    }
}
