using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Atlassian.Jira;
using Moq;
using Tinkoff.ISA.DAL.Jira;
using Xunit;
using static Atlassian.Jira.Jira;

namespace Tinkoff.ISA.DAL.UnitTests.Jira
{
    public class JiraClientTests
    {
        private readonly Mock<IJiraClientWrapper> _jiraClientWrapperMock;
        private readonly Mock<IIssueService> _issueServiceMock;
        private readonly IJiraClient _classThatWeActuallyTesting;
        private static readonly string[] ProjectNames = {"test"};
        private readonly Issue _myIssue;

        public JiraClientTests()
        {
            _jiraClientWrapperMock = new Mock<IJiraClientWrapper>();
            _issueServiceMock = new Mock<IIssueService>();
            _classThatWeActuallyTesting = new JiraClient(_jiraClientWrapperMock.Object);

            var client = CreateRestClient("https://your_jira_address/");
            var fields = new CreateIssueFields(ProjectNames[0]);
            _myIssue = new Issue(client, fields);
        }

        [Fact]
        public async void GetAllIssuesAsync_ProjectNameNull_ArgumentException()
        {
            //act, assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _classThatWeActuallyTesting.GetAllIssuesAsync(null, 10, 0));
        }

       [Fact]
        public async void GetLatestIssuesAsync_ProjectNameNull_ArgumentException()
        {
            //act, assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _classThatWeActuallyTesting.GetLatestIssuesAsync(null, DateTime.Now, 10, 0));
        } 

        [Fact]
        public async void GetAllIssuesAsync_CorrectParams_ShouldCallGetIssuesFromJqlAsync()
        {
            var result = new MockPagedQueryResult<Issue>(new [] {_myIssue});
            var expectedJqlQuery = $"project in ({string.Join(", ", ProjectNames)}) " +
                                   $"ORDER BY updated ASC";
            string actualJqlQuery = null;

            _issueServiceMock
                .Setup(s => s.GetIssuesFromJqlAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int>(),
                    default(CancellationToken)))
                .Callback((string query, int? a, int b, CancellationToken token) => actualJqlQuery = query)
                .ReturnsAsync(result);

            _jiraClientWrapperMock
                .SetupGet(s => s.Issues)
                .Returns(_issueServiceMock.Object); 

            //act 
            await _classThatWeActuallyTesting.GetAllIssuesAsync(ProjectNames, 10, 0);

            //assert
            Assert.Equal(expectedJqlQuery, actualJqlQuery);
        }

        [Fact]
        public async void GetLatestIssuesAsync_CorrectParams_ShouldCallGetIssuesFromJqlAsync()
        {
            var dateTimeValue = DateTime.Now;
            var result = new MockPagedQueryResult<Issue>(new[] { _myIssue });
            var expectedJqlQuery =
                $"project in ({string.Join(", ", ProjectNames)}) " +
                $"AND updated >= \"{dateTimeValue.ToString("yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture)}\" " +
                $"ORDER BY updated ASC";
            string actualJqlQuery = null;

            _issueServiceMock
                .Setup(s => s.GetIssuesFromJqlAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int>(),
                    default(CancellationToken)))
                .Callback((string query, int? a, int b, CancellationToken token) => actualJqlQuery = query)
                .ReturnsAsync(result);

            _jiraClientWrapperMock
                .SetupGet(s => s.Issues)
                .Returns(_issueServiceMock.Object);

            //act 
            await _classThatWeActuallyTesting.GetLatestIssuesAsync(ProjectNames, DateTime.Now, 10, 0);

            //assert
            Assert.Equal(expectedJqlQuery, actualJqlQuery);
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
