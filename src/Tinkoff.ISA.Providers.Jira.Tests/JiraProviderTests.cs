using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Atlassian.Jira;
using Atlassian.Jira.Remote;
using Moq;
using Tinkoff.ISA.Core;
using Xunit;
using JiraClient = Atlassian.Jira.Jira;

namespace Tinkoff.ISA.Providers.Jira.Tests
{
    public class JiraProviderTests
    {
        private readonly IKnowledgeProvider _jiraProvider;
        private readonly Mock<IIssueService> _issueServiceMock = new Mock<IIssueService>();
        private readonly JiraClient _jiraClientMock;
        
        public JiraProviderTests()
        {
            var settings = new JiraProviderSettings
            {
                BaseUrl = "https://base_url",
                BatchSize = 100,
                ProjectNames = new List<string>(),
                UserName = "user",
                Token = "sdv78f8sf"
            };

            _jiraClientMock = CreateJiraClient(settings);
            _jiraProvider = new JiraKnowledgeProvider(_jiraClientMock, settings);
        }

        [Fact]
        public async Task GetKnowledgeBatch_UploadedToDate_ShouldBeAccordingToTheLastDocument()
        {
            // Arrange
            var firstIssueDate = DateTime.Now;
            var secondIssueDate = firstIssueDate.AddSeconds(5);
            var thirdIssueDate = firstIssueDate.AddSeconds(10);
            
            var firstIssue = CreateIssue(firstIssueDate);
            var secondIssue = CreateIssue(secondIssueDate);
            var thirdIssue = CreateIssue(thirdIssueDate);
            
            // it implies that issues come from jira sorted by time in ascending order
            var issues = new List<Issue>
            {
                firstIssue,
                secondIssue,
                thirdIssue
            };

            var mockPagedQueryResult = new MockPagedQueryResult<Issue>(issues);

            _issueServiceMock
                .Setup(service => service.GetIssuesFromJqlAsync(It.IsAny<IssueSearchOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockPagedQueryResult);

            // Act
            var knowledgeBatch = await _jiraProvider.GetKnowledgeBatch(new KnowledgeRequest
            {
                StartAt = 0,
                StartDate = DateTimeOffset.Now.AddDays(-1)
            });

            // Assert
            Assert.Equal(thirdIssueDate, knowledgeBatch.UploadedToDate);
        }

        [Theory]
        [InlineData(10, 11, false)]
        [InlineData(10, 10, true)]
        public async Task GetKnowledgeBatch_IsLastBatchProperty_ShouldBeSetCorrectly(
            int numberOfReturnedIssues, int numberOfTotalIssues, bool shouldBeLastBatch)
        {
            // Arrange
            var issues = new List<Issue>();
            var issueUpdatedDate = DateTime.Now.AddDays(-1);
            
            for (var i = 0; i < numberOfReturnedIssues; i++)
            {
                var issue = CreateIssue(issueUpdatedDate);
                issues.Add(issue);
                issueUpdatedDate = issueUpdatedDate.AddSeconds(10);
            }
            
            var mockPagedQueryResult = new MockPagedQueryResult<Issue>(issues)
            {
                TotalItems = numberOfTotalIssues
            };
            
            _issueServiceMock
                .Setup(service => service.GetIssuesFromJqlAsync(It.IsAny<IssueSearchOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockPagedQueryResult);
            
            // Act
            var knowledgeBatch = await _jiraProvider.GetKnowledgeBatch(new KnowledgeRequest
            {
                StartAt = 0,
                StartDate = DateTimeOffset.Now.AddDays(-1)
            });
            
            // Assert
            Assert.Equal(shouldBeLastBatch, knowledgeBatch.IsLastBatch);
        }

        private Issue CreateIssue(DateTime updateDate)
        {
            var firstRemoteIssue = new RemoteIssue
            {
                updated = updateDate 
            };
            
            return new Issue(_jiraClientMock, firstRemoteIssue);
        }

        private JiraClient CreateJiraClient(JiraProviderSettings settings)
        {
            var serviceLocator = new ServiceLocator();
            serviceLocator.Register(() => _issueServiceMock.Object);
            serviceLocator.Register(() => new Mock<IJiraRestClient>().Object);
            
            var credentials = new JiraCredentials(settings.UserName, settings.Token);
            
            return new JiraClient(serviceLocator, credentials);
        }
        
        private class MockPagedQueryResult<T> : IPagedQueryResult<T>
        {
            private readonly IEnumerable<T> _enumerable;

            public MockPagedQueryResult(IEnumerable<T> enumerable)
            {
                _enumerable = enumerable;
            }
            
            public int ItemsPerPage { get; set; }
            public int StartAt { get; set; }
            public int TotalItems { get; set; }

            public IEnumerator<T> GetEnumerator()
            {
                return _enumerable.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}