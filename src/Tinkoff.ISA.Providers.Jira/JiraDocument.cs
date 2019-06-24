using Tinkoff.ISA.Core.Documents;

namespace Tinkoff.ISA.Providers.Jira
{
    public class JiraDocument : ISearchableByTitleDocument
    {
        public string Title { get; set; }
        
        public string Text { get; set; }

        public string Link { get; set; }
    }
}