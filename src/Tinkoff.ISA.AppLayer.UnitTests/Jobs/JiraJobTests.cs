using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Atlassian.Jira;
using Atlassian.Jira.Remote;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Tinkoff.ISA.AppLayer.Jobs;
using Tinkoff.ISA.DAL.Elasticsearch.Client;
using Tinkoff.ISA.DAL.Elasticsearch.Request;
using Tinkoff.ISA.DAL.Jira;
using Tinkoff.ISA.DAL.Storage.Dao.Application;
using Tinkoff.ISA.Domain.Application;
using Tinkoff.ISA.Infrastructure.Settings;
using Xunit;
using static Atlassian.Jira.Jira;

namespace Tinkoff.ISA.AppLayer.UnitTests.Jobs
{
    
    public class JiraJobTests
    {
        private readonly Mock<IJiraClient> _jiraClientMock;
        private readonly Mock<IElasticsearchClient> _elasticsearchClientMock;
        private readonly Mock<IApplicationPropertyDao> _applicationPropertyDaoMock;
        private readonly Mock<IOptions<JiraSettings>> _settingsMock;
        private readonly JiraJob _job;
        private readonly MockPagedQueryResult<Issue> _response;

        public JiraJobTests()
        {
            _applicationPropertyDaoMock = new Mock<IApplicationPropertyDao>();
            _elasticsearchClientMock = new Mock<IElasticsearchClient>();
            _jiraClientMock = new Mock<IJiraClient>();
            
            var loggerMock = new Mock<ILogger<JiraJob>>();
            _settingsMock = new Mock<IOptions<JiraSettings>>();
            _settingsMock
                .SetupGet(m => m.Value)
                .Returns(() => new JiraSettings
                {
                    User = "user",
                    Password = "pass",
                    BaseAddress = "https://your_jira_address/",
                    BatchSize = 3,
                    ProjectNames = new[]{ "test", "test1", "test2" }
                });

            _job = new JiraJob(loggerMock.Object,
                _elasticsearchClientMock.Object, 
                _applicationPropertyDaoMock.Object, 
                _jiraClientMock.Object, 
                _settingsMock.Object);

            var client = CreateRestClient("https://your_jira_address/");

            var fields = new RemoteIssue
            {
                updated = DateTime.UtcNow,
                id = "id1",
                key = "key1",
                description = "description1",
                summary = "summary1"
            };
            var myIssue1 = new Issue(client, fields);

            fields = new RemoteIssue
            {
                updated = DateTime.UtcNow,
                id = "id2",
                key = "key2",
                description = "description2",
                summary = "summary2"
            };
            var myIssue2 = new Issue(client, fields);

            fields = new RemoteIssue
            {
                updated = DateTime.UtcNow,
                id = "id3",
                key = "key3",
                description = "description3",
                summary = "summary3"
            };
            var myIssue3 = new Issue(client, fields);

            var issues = new List<Issue> {myIssue1, myIssue2, myIssue3};
            var orderedIssues = issues.OrderBy(i => i.Updated);

            _response = new MockPagedQueryResult<Issue>(orderedIssues) {TotalItems = 3};
        }

        [Fact]
        public async Task StartJob_FirstTime_ShouldChooseEarliestDate()
        {
            //arange
            var actualDate = DateTime.Today;
            _applicationPropertyDaoMock
                .Setup(s => s.GetAsync())
                .ReturnsAsync(new ApplicationProperty());
            _applicationPropertyDaoMock
                .Setup(s => s.UpsertPropertyAsync(It.IsAny<Expression<Func<ApplicationProperty, DateTime>>>(), It.IsAny<DateTime>()))
                .Returns(Task.CompletedTask);
            _jiraClientMock
                .Setup(s => s.GetLatestIssuesAsync(It.IsAny<string[]>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(_response)
                .Callback((string[] names, DateTime date, int issuesPerRequest, int startAt) => actualDate = date);
            _elasticsearchClientMock
                .Setup(s => s.UpsertManyAsync(It.IsAny<JiraElasticUpsertRequest>()))
                .Returns(Task.CompletedTask);
            
            //act 
            await _job.StartJob();

            //assert
            Assert.Equal(actualDate, DateTime.MinValue.ToLocalTime(), TimeSpan.FromMilliseconds(500));
        }

        [Fact]
        public async Task StartJob_notFirstTime_ShouldChooseDateAccordingToTheSettings()
        {
            //arange
            var actualDate = DateTime.Now;
            var expectedDate = DateTime.Today;

            _applicationPropertyDaoMock
                .Setup(s => s.GetAsync())
                .ReturnsAsync(new ApplicationProperty { JiraJobLastUpdate = expectedDate });
            _applicationPropertyDaoMock
                .Setup(s => s.UpsertPropertyAsync(It.IsAny<Expression<Func<ApplicationProperty, DateTime>>>(), It.IsAny<DateTime>()))
                .Returns(Task.CompletedTask);
            _jiraClientMock
                .Setup(s => s.GetLatestIssuesAsync(It.IsAny<string[]>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(_response)
                .Callback((string[] names, DateTime date, int issuesPerRequest, int startAt) => actualDate = date);
            _elasticsearchClientMock
                .Setup(s => s.UpsertManyAsync(It.IsAny<JiraElasticUpsertRequest>()))
                .Returns(Task.CompletedTask);

            //act 
            await _job.StartJob();

            //assert
            Assert.Equal(actualDate, expectedDate);
        }

        [Fact]
        public async Task StartJob_RegularInvocation_ShouldUpsertDateLatestFromResponse()
        {
            //arange
            var actualUpsertedDate = DateTime.MaxValue;

            _applicationPropertyDaoMock
                .Setup(s => s.GetAsync())
                .ReturnsAsync(new ApplicationProperty());
            _applicationPropertyDaoMock
                .Setup(s => s.UpsertPropertyAsync(It.IsAny<Expression<Func<ApplicationProperty, DateTime>>>(),
                    It.IsAny<DateTime>()))
                .Returns(Task.CompletedTask)
                .Callback((Expression<Func<ApplicationProperty, DateTime>> expr, DateTime date) => actualUpsertedDate = date);
            _jiraClientMock
                .Setup(s => s.GetLatestIssuesAsync(It.IsAny<string[]>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(_response);
            _elasticsearchClientMock
                .Setup(s => s.UpsertManyAsync(It.IsAny<JiraElasticUpsertRequest>()))
                .Returns(Task.CompletedTask);

            //act 
            await _job.StartJob();

            //asert
            Assert.Equal(actualUpsertedDate, _response.Last().Updated);
        }

        [Fact]
        public async Task StartJob_RegularInvocation_ShouldGetAllIssuesUpdatedAtSameTime()
        {
            //arange
            var dateTime = DateTime.Now;

            var client = CreateRestClient("https://your_jira_address/");
            var fields = new RemoteIssue
            {
                updated = dateTime,
                id = "id4",
                key = "key4",
                description = "description4",
                summary = "summary4"
            };
            var myIssue = new Issue(client, fields);

            var firstIssues = new List<Issue> { myIssue, myIssue, myIssue };
            var secondIssues = new List<Issue> { myIssue };

            var firstResponse = new MockPagedQueryResult<Issue>(firstIssues) { TotalItems = 4 };
            var secondResponse = new MockPagedQueryResult<Issue>(secondIssues) { TotalItems = 1 };

            _applicationPropertyDaoMock
                .Setup(s => s.GetAsync())
                .ReturnsAsync(new ApplicationProperty {JiraJobLastUpdate = dateTime});
            _applicationPropertyDaoMock
                .Setup(s => s.UpsertPropertyAsync(It.IsAny<Expression<Func<ApplicationProperty, DateTime>>>(), It.IsAny<DateTime>()))
                .Returns(Task.CompletedTask);
            _jiraClientMock
                .SetupSequence(s => s.GetLatestIssuesAsync(It.IsAny<string[]>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(firstResponse)
                .ReturnsAsync(secondResponse);
            _elasticsearchClientMock
                .Setup(s => s.UpsertManyAsync(It.IsAny<JiraElasticUpsertRequest>()))
                .Returns(Task.CompletedTask);

            //act 
            await _job.StartJob();

            //assert
            _jiraClientMock.Verify(v => v
                    .GetLatestIssuesAsync(It.IsAny<string[]>(), It.IsAny<DateTime>(), It.IsAny<int>(), 0),
                Times.Once);
            _jiraClientMock.Verify(v => v
                    .GetLatestIssuesAsync(It.IsAny<string[]>(), It.IsAny<DateTime>(), It.IsAny<int>(), _settingsMock.Object.Value.BatchSize),
                Times.Once);
            _jiraClientMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task StartJob_RegularInvocation_ShouldUpsertTwice()
        {
            //arange
            var client = CreateRestClient("https://your_jira_address/");

            var fields = new RemoteIssue {
                updated = DateTime.UtcNow,
                id = "id4",
                key = "key4",
                description = "description4",
                summary = "summary4"
            };
            var myIssue = new Issue(client, fields);
            
            var firstIssues = new List<Issue> { myIssue, myIssue, myIssue};
            var secondIssues = new List<Issue> { myIssue};

            var firstResponse = new MockPagedQueryResult<Issue>(firstIssues) { TotalItems = 4 };
            var secondResponse = new MockPagedQueryResult<Issue>(secondIssues) { TotalItems = 1 };

            _applicationPropertyDaoMock
                .Setup(s => s.GetAsync())
                .ReturnsAsync(new ApplicationProperty());
            _applicationPropertyDaoMock
                .Setup(s => s.UpsertPropertyAsync(It.IsAny<Expression<Func<ApplicationProperty, DateTime>>>(), It.IsAny<DateTime>()))
                .Returns(Task.CompletedTask);
            _jiraClientMock
                .SetupSequence(s => s.GetLatestIssuesAsync(It.IsAny<string[]>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(firstResponse)
                .ReturnsAsync(secondResponse);
            _elasticsearchClientMock
                .Setup(s => s.UpsertManyAsync(It.IsAny<JiraElasticUpsertRequest>()))
                .Returns(Task.CompletedTask);
            
            //act 
            await _job.StartJob();

            //assert
            _jiraClientMock.Verify(v => v
                .GetLatestIssuesAsync(It.IsAny<string[]>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()), 
                Times.Exactly(2));
            _jiraClientMock.VerifyNoOtherCalls();

            _elasticsearchClientMock.Verify(v => v
                .UpsertManyAsync(It.IsAny<JiraElasticUpsertRequest>()),
                Times.Exactly(2));
            _elasticsearchClientMock.VerifyNoOtherCalls();

            _applicationPropertyDaoMock.Verify(v => v
                .UpsertPropertyAsync(It.IsAny<Expression<Func<ApplicationProperty, DateTime>>>(), It.IsAny<DateTime>()),
                Times.Exactly(2));
            _applicationPropertyDaoMock.Verify(v => v
                .GetAsync(),
                Times.Exactly(1));
            _applicationPropertyDaoMock.VerifyNoOtherCalls();
        }
    }
    public class MockPagedQueryResult<T> : IPagedQueryResult<T>
    {
        private readonly IEnumerable<T> _enumerable;

        public MockPagedQueryResult(IEnumerable<T> enumerable)
        {
            _enumerable = enumerable;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _enumerable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _enumerable.GetEnumerator();
        }

        public int ItemsPerPage { get; set; }
        public int StartAt { get; set; }
        public int TotalItems { get; set; }
    }
}
