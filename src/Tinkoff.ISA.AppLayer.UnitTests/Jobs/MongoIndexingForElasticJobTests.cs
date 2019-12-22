using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Tinkoff.ISA.AppLayer.Jobs;
using Tinkoff.ISA.AppLayer.Search;
using Tinkoff.ISA.DAL.Elasticsearch.Client;
using Tinkoff.ISA.DAL.Elasticsearch.Request;
using Tinkoff.ISA.DAL.Storage.Dao.Application;
using Tinkoff.ISA.Domain.Application;
using Tinkoff.ISA.Domain.Search;
using Xunit;

namespace Tinkoff.ISA.AppLayer.UnitTests.Jobs
{
    public class MongoIndexingForElasticJobTests
    {
        private readonly MongoIndexingForElasticJob _mongoIndexingForElasticJob;
        private readonly Mock<IElasticSearchClient> _elasticsearchClientMock;
        private readonly Mock<IApplicationPropertyDao> _applicationPropertyDaoMock;
        private readonly Mock<ISearchableTextService> _searchableTextServiceMock;
        
        public MongoIndexingForElasticJobTests()
        {
            var loggerMock = new Mock<ILogger<MongoIndexingForElasticJob>>();
            
            _elasticsearchClientMock = new Mock<IElasticSearchClient>();
            _applicationPropertyDaoMock = new Mock<IApplicationPropertyDao>();
            _searchableTextServiceMock = new Mock<ISearchableTextService>();
            
            _mongoIndexingForElasticJob = new MongoIndexingForElasticJob(_searchableTextServiceMock.Object,
                _elasticsearchClientMock.Object,
                _applicationPropertyDaoMock.Object,
                loggerMock.Object);
        }

        [Fact]
        public async Task StartJob_FirstTime_ShouldSpecifyTheEarliestDate()
        {
            // Arrange
            _applicationPropertyDaoMock.Setup(d => d.GetAsync()).ReturnsAsync(new ApplicationProperty());
            _searchableTextServiceMock.Setup(s => s.GetAnswersAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new List<SearchableAnswer>());
            _searchableTextServiceMock.Setup(s => s.GetQuestionsWithAnswersAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new List<SearchableQuestion>());
            
            // Act
            await _mongoIndexingForElasticJob.StartJob();

            // Assert
            _searchableTextServiceMock.Verify(s =>
                s.GetAnswersAsync(It.Is<DateTime>(d => d.Equals(DateTime.MinValue))), Times.Once);
            _searchableTextServiceMock.Verify(s =>
                s.GetQuestionsWithAnswersAsync(It.Is<DateTime>(d => d.Equals(DateTime.MinValue))), Times.Once);
        }

        [Fact]
        public async Task StartJob_NotFirstTime_ShouldSpecifyDateAccordingToSettings()
        {
            // Arrange
            var expectedDate = new DateTime(2019, 1, 1, 1, 1, 1);
            
            _applicationPropertyDaoMock.Setup(d => d.GetAsync()).ReturnsAsync(new ApplicationProperty
            {
                LastMongoIndexing = expectedDate
            });
            _searchableTextServiceMock.Setup(s => s.GetAnswersAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new List<SearchableAnswer>());
            _searchableTextServiceMock.Setup(s => s.GetQuestionsWithAnswersAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new List<SearchableQuestion>());
            
            // Act
            await _mongoIndexingForElasticJob.StartJob();

            // Assert
            _searchableTextServiceMock.Verify(s =>
                s.GetAnswersAsync(It.Is<DateTime>(d => d.Equals(expectedDate))), Times.Once);
            _searchableTextServiceMock.Verify(s =>
                s.GetQuestionsWithAnswersAsync(It.Is<DateTime>(d => d.Equals(expectedDate))), Times.Once);
        }

        [Fact]
        public async Task StartJob_ThereAreNoNewAnswersAndQuestionsInDb_ShouldNotIndexing()
        {
            // Arrange
            _applicationPropertyDaoMock.Setup(d => d.GetAsync()).ReturnsAsync(new ApplicationProperty());
            _searchableTextServiceMock.Setup(s => s.GetAnswersAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new List<SearchableAnswer>());
            _searchableTextServiceMock.Setup(s => s.GetQuestionsWithAnswersAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new List<SearchableQuestion>());
            
            // Act
            await _mongoIndexingForElasticJob.StartJob();
            
            // Assert
            _elasticsearchClientMock.Verify(c => c.UpsertManyAsync(It.IsAny<ElasticUpsertRequest<SearchableQuestion>>()),
                Times.Never);
            _elasticsearchClientMock.Verify(c => c.UpsertManyAsync(It.IsAny<ElasticUpsertRequest<SearchableAnswer>>()),
                Times.Never);
        }
        
        [Fact]
        public async Task StartJob_ThereAreNewAnswersAndQuestionsInDb_ShouldIndexing()
        {
            // Arrange
            var answers = new List<SearchableAnswer>
            {
                new SearchableAnswer{Id = "id1", Text = "text1"},
                new SearchableAnswer{Id = "id2", Text = "text2"}
            };
            
            var questions = new List<SearchableQuestion>
            {
                new SearchableQuestion{Id = "id1", Text = "text1"},
                new SearchableQuestion{Id = "id2", Text = "text2"}
            };
            
            _applicationPropertyDaoMock.Setup(d => d.GetAsync()).ReturnsAsync(new ApplicationProperty());
            _searchableTextServiceMock.Setup(s => s.GetAnswersAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(answers);
            _searchableTextServiceMock.Setup(s => s.GetQuestionsWithAnswersAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(questions);
            
            // Act
            await _mongoIndexingForElasticJob.StartJob();
            
            // Assert
            _elasticsearchClientMock.Verify(c => c.UpsertManyAsync(It.IsAny<ElasticUpsertRequest<SearchableQuestion>>()),
                Times.Once);
            _elasticsearchClientMock.Verify(c => c.UpsertManyAsync(It.IsAny<ElasticUpsertRequest<SearchableAnswer>>()),
                Times.Once);
            _elasticsearchClientMock.VerifyNoOtherCalls();
        }
    }
}