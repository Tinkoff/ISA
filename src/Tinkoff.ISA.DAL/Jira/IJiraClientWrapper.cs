using Atlassian.Jira;

namespace Tinkoff.ISA.DAL.Jira
{
    public interface IJiraClientWrapper
    {
        IIssueService Issues { get; }
    }
}
