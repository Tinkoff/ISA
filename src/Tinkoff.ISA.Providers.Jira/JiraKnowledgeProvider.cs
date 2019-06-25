using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Atlassian.Jira;
using Tinkoff.ISA.Core;
using Tinkoff.ISA.Core.Documents;
using JiraClient = Atlassian.Jira.Jira;

namespace Tinkoff.ISA.Providers.Jira
{
    public class JiraKnowledgeProvider : IKnowledgeProvider
    {
        private readonly JiraClient _jiraClient;
        private readonly List<string> _projectNames;
        private readonly int _batchSize;
        private readonly string _baseUrl;

        public JiraKnowledgeProvider(JiraClient jiraClient, JiraProviderSettings settings)
        {
            _jiraClient = jiraClient;
            _baseUrl = settings.BaseUrl;
            _projectNames = settings.ProjectNames;
            _batchSize = settings.BatchSize;
        }
        
        public async Task<IKnowledgeBatch<ISearchableDocument>> GetKnowledgeBatch(KnowledgeRequest request)
        {
            var issuesPagedResult = await GetIssuesAsync(request);

            var uploadedToDate = GetDateOfUpdateOfTheLatestDocument(issuesPagedResult);
            var documents = MapToJiraDocuments(issuesPagedResult);

            var isLastBatch = issuesPagedResult.TotalItems == documents.Count;
            
            return new KnowledgeBatch<JiraDocument>(documents, uploadedToDate, isLastBatch);
        }

        private static string EnsureTrailingSlash(string baseUrl)
        {
            return baseUrl.EndsWith("/", StringComparison.InvariantCulture) ? baseUrl : baseUrl + "/";
        }

        private IList<JiraDocument> MapToJiraDocuments(IEnumerable<Issue> issues)
        {
            var baseUrl = EnsureTrailingSlash(_baseUrl);
                
            return issues.Select(issue => new JiraDocument
            {
                Title = issue.Summary,
                Text = issue.Description,
                Link = $"{baseUrl}browse/{issue.Key}"
            }).ToList();
        }

        private Task<IPagedQueryResult<Issue>> GetIssuesAsync(KnowledgeRequest request)
        {
            var startDateUtc = request.StartDate.UtcDateTime.ToString("yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture);
            
            var jql = $"project in ({string.Join(", ", _projectNames)}) " +
                      $"AND updated >= \"{startDateUtc}\" " +
                      "ORDER BY updated ASC";
            
            var options = new IssueSearchOptions(jql)
            {
                StartAt = request.StartAt,
                MaxIssuesPerRequest = _batchSize
            };
            
            return _jiraClient.Issues.GetIssuesFromJqlAsync(options);
        }

        private DateTime GetDateOfUpdateOfTheLatestDocument(IEnumerable<Issue> issues)
        {
            var dateOfUpdate = issues.Last().Updated;

            if (dateOfUpdate.HasValue)
            {
                return dateOfUpdate.Value;
            }

            throw new ArgumentNullException(nameof(dateOfUpdate));
        }
    }
}