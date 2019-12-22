using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Tinkoff.ISA.AppLayer.Jobs;
using Tinkoff.ISA.DAL.Confluence;
using Tinkoff.ISA.DAL.Confluence.Dtos;
using Tinkoff.ISA.DAL.Elasticsearch.Client;
using Tinkoff.ISA.DAL.Elasticsearch.Request;
using Tinkoff.ISA.DAL.Storage.Dao.Application;
using Tinkoff.ISA.Domain.Application;
using Tinkoff.ISA.Infrastructure.Settings;
using Xunit;

namespace Tinkoff.ISA.AppLayer.UnitTests.Jobs
{
    public class ConfluenceJobTests
    {
        private readonly Mock<IConfluenceHttpClient> _confluenceHttpClientMock;
        private readonly Mock<IElasticSearchClient> _elasticsearchClientMock;
        private readonly Mock<IApplicationPropertyDao> _applicationPropertyDaoMock;
        private readonly ConfluenceJob _job;
        private readonly ContentResponse _response;

        public ConfluenceJobTests()
        {
            _confluenceHttpClientMock = new Mock<IConfluenceHttpClient>();
            _elasticsearchClientMock = new Mock<IElasticSearchClient>();
            _applicationPropertyDaoMock = new Mock<IApplicationPropertyDao>();
            var settingsMock = new Mock<IOptions<ConfluenceSettings>>();
            settingsMock
                .SetupGet(m => m.Value)
                .Returns(() => new ConfluenceSettings
                {
                    User = "user",
                    Password = "password",
                    SpaceKeys = new []{"SK"},
                    BaseAddress = "https://your_wiki_address",
                    BatchSize = 100,
                    HttpTimeoutMilliseconds = 1000
                });
            var loggerMock = new Mock<ILogger<ConfluenceJob>>();
            _job = new ConfluenceJob(_confluenceHttpClientMock.Object, _elasticsearchClientMock.Object,
                _applicationPropertyDaoMock.Object, settingsMock.Object, loggerMock.Object);

            var firstPage = new ContentDto
            {
                Id = "123",
                Title = "title 1",
                Body = new ContentBodyDto
                {
                    View = new ViewRepresentationDto
                    {
                        Value = "body goes here 1"
                    }
                },
                Version = new VersionDto { When = DateTime.UtcNow },
                Links = new LinksDto { Webui = "link 1" },
            };

            var secondPage = new ContentDto
            {
                Id = "234",
                Title = "title 2",
                Body = new ContentBodyDto
                {
                    View = new ViewRepresentationDto
                    {
                        Value = "body goes here 2"
                    }
                },
                Version = new VersionDto { When = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)) },
                Links = new LinksDto { Webui = "link 2" },
            };

            _response = new ContentResponse
            {
                Links = new LinksDto(),
                Results = new List<ContentDto> {firstPage, secondPage}.OrderBy(p => p.Version.When).ToList()
            };
        }

        [Fact]
        public async Task StartJob_NoAppProperty_ShouldSpecifyTheEarliestDate()
        {
            // Arrange
            var actualDate = DateTime.Today;
            _response.Links.Next = null;

            _applicationPropertyDaoMock.Setup(m => m.GetAsync()).ReturnsAsync((ApplicationProperty)null);
            _confluenceHttpClientMock.Setup(m => m.GetLatestPagesAsync(It.IsAny<string[]>(), It.IsAny<DateTime>()))
                .ReturnsAsync(_response)
                .Callback((string[] spaceKeys, DateTime date) => actualDate = date);
            _elasticsearchClientMock.Setup(m => m.UpsertManyAsync(It.IsAny<ConfluenceElasticUpsertRequest>()))
                .Returns(Task.CompletedTask);

            _applicationPropertyDaoMock.Setup(m =>
                    m.UpsertPropertyAsync(It.IsAny<Expression<Func<ApplicationProperty, DateTime>>>(),
                        It.IsAny<DateTime>()))
                .Returns(Task.CompletedTask);

            // Act
            await _job.StartJob();

            // Assert
            Assert.Equal(DateTime.MinValue, actualDate, TimeSpan.FromMilliseconds(500));
        }

        [Fact]
        public async Task StartJob_NotFirstTime_ShouldSpecifyDateAccordingToSettings()
        {
            // Arrange
            var actualDate = DateTime.Today;
            var dateFromSettings = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1));
            var expectedDate = dateFromSettings.ToLocalTime();
            
            _response.Links.Next = null;

            _applicationPropertyDaoMock.Setup(m => m.GetAsync())
                .ReturnsAsync(new ApplicationProperty {ConfluenceJobLastUpdate = dateFromSettings});
            _confluenceHttpClientMock.Setup(m => m.GetLatestPagesAsync(It.IsAny<string[]>(), It.IsAny<DateTime>()))
                .ReturnsAsync(_response)
                .Callback((string[] spaceKeys, DateTime date) => actualDate = date);
            _elasticsearchClientMock.Setup(m => m.UpsertManyAsync(It.IsAny<ConfluenceElasticUpsertRequest>()))
                .Returns(Task.CompletedTask);

            _applicationPropertyDaoMock.Setup(m =>
                    m.UpsertPropertyAsync(It.IsAny<Expression<Func<ApplicationProperty, DateTime>>>(),
                        It.IsAny<DateTime>()))
                .Returns(Task.CompletedTask);

            // Act
            await _job.StartJob();

            // Assert
            Assert.Equal(expectedDate, actualDate, TimeSpan.FromMilliseconds(500));
        }

        [Fact]
        public async Task StartJob_RegularInvocation_ShouldUpsertDateLatestFromResponse()
        {
            // Arrange
            var actualUpsertedDate = DateTime.Today;
            _response.Links.Next = null;

            _applicationPropertyDaoMock.Setup(m => m.GetAsync())
                .ReturnsAsync(new ApplicationProperty());
            _confluenceHttpClientMock.Setup(m => m.GetLatestPagesAsync(It.IsAny<string[]>(), It.IsAny<DateTime>()))
                .ReturnsAsync(_response);
            _elasticsearchClientMock.Setup(m => m.UpsertManyAsync(It.IsAny<ConfluenceElasticUpsertRequest>()))
                .Returns(Task.CompletedTask);

            _applicationPropertyDaoMock.Setup(m =>
                    m.UpsertPropertyAsync(It.IsAny<Expression<Func<ApplicationProperty, DateTime>>>(),
                        It.IsAny<DateTime>()))
                .Returns(Task.CompletedTask)
                .Callback((Expression<Func<ApplicationProperty, DateTime>> expr, DateTime date) =>
                    actualUpsertedDate = date);

            // Act
            await _job.StartJob();

            // Assert
            Assert.Equal(actualUpsertedDate, _response.Results.Last().Version.When, TimeSpan.FromMilliseconds(100));
        }

        [Fact]
        public async Task StartJob_NextLinkInResponse_ShouldUpsertTwice()
        {
            _response.Links.Next = "link goes here";
            var nextPortion = new ContentResponse
            {
                Results = new List<ContentDto>
                {
                    new ContentDto
                    {
                        Id = "789",
                        Title = "title 3",
                        Body = new ContentBodyDto
                        {
                            View = new ViewRepresentationDto{ Value = "body goes here 3"}
                        },
                        Links = new LinksDto { Webui = "link 3" },
                    }
                }
            };

            _applicationPropertyDaoMock.Setup(m => m.GetAsync())
                .ReturnsAsync(new ApplicationProperty());
            _confluenceHttpClientMock.Setup(m => m.GetLatestPagesAsync(It.IsAny<string[]>(), It.IsAny<DateTime>()))
                .ReturnsAsync(_response);
            _elasticsearchClientMock.Setup(m => m.UpsertManyAsync(It.IsAny<ConfluenceElasticUpsertRequest>()))
                .Returns(Task.CompletedTask);
            _confluenceHttpClientMock.Setup(m => m.GetNextBatchAsync(It.IsAny<string>()))
                .ReturnsAsync(nextPortion);
            _applicationPropertyDaoMock.Setup(m =>
                    m.UpsertPropertyAsync(It.IsAny<Expression<Func<ApplicationProperty, DateTime>>>(),
                        It.IsAny<DateTime>()))
                .Returns(Task.CompletedTask);

            // Act
            await _job.StartJob();

            // Assert
            _confluenceHttpClientMock.Verify(m => m.GetNextBatchAsync(It.Is<string>(url => url == _response.Links.Next)));
            _elasticsearchClientMock.Verify(m => m.UpsertManyAsync(It.IsAny<ConfluenceElasticUpsertRequest>()),
                Times.Exactly(2));
            _elasticsearchClientMock.VerifyNoOtherCalls();
        }
    }
}
