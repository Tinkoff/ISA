using Atlassian.Jira;
using Microsoft.Extensions.Options;
using Tinkoff.ISA.Infrastructure.Settings;
using static Atlassian.Jira.Jira;
using JiraNativeClient = Atlassian.Jira.Jira;

namespace Tinkoff.ISA.DAL.Jira
{
    internal class JiraClientWrapper : IJiraClientWrapper
    {
        private readonly JiraNativeClient _jiraClient;

        public JiraClientWrapper(IOptions<JiraSettings> jiraSettings)
        {
            _jiraClient = CreateRestClient(
                jiraSettings.Value.BaseAddress,
                jiraSettings.Value.User,
                jiraSettings.Value.Password);
        }

        public IIssueService Issues => _jiraClient.Issues;
    }
}
