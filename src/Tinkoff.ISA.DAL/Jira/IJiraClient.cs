using System;
using System.Threading.Tasks;
using Atlassian.Jira;

namespace Tinkoff.ISA.DAL.Jira
{
    public interface IJiraClient
    {
        Task<IPagedQueryResult<Issue>> GetAllIssuesAsync(string[] projectNames, int issuesPerRequest, int startAt);

        Task<IPagedQueryResult<Issue>> GetLatestIssuesAsync(string[] projectNames, DateTime dateTime, int issuesPerRequest, int startAt);
    }
}
