using System;
using System.Globalization;
using System.Threading.Tasks;
using Atlassian.Jira;

namespace Tinkoff.ISA.DAL.Jira
{
    internal class JiraClient : IJiraClient
    {
        private readonly IJiraClientWrapper _jiraClient;

        public JiraClient(IJiraClientWrapper jiraClient)
        {
            _jiraClient = jiraClient;
        }

        public Task<IPagedQueryResult<Issue>> GetAllIssuesAsync(string[] projectNames, int issuesPerRequest, int startAt)
        {
            if (projectNames == null) throw new ArgumentNullException(nameof(projectNames));

            return _jiraClient.Issues.GetIssuesFromJqlAsync(
                $"project in ({string.Join(", ", projectNames)}) " +
                $"ORDER BY updated ASC", issuesPerRequest, startAt);
        }

        public Task<IPagedQueryResult<Issue>> GetLatestIssuesAsync(string[] projectNames, DateTime dateTime, int issuesPerRequest, int startAt)
        {
            if (projectNames == null) throw new ArgumentNullException(nameof(projectNames));

            return _jiraClient.Issues.GetIssuesFromJqlAsync(
                $"project in ({string.Join(", ", projectNames)}) " +
                $"AND updated >= \"{dateTime.ToString("yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture)}\" " +
                $"ORDER BY updated ASC",
                issuesPerRequest, 
                startAt);
        }
    }
}
