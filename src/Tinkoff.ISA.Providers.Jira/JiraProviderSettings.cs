using System.Collections.Generic;

namespace Tinkoff.ISA.Providers.Jira
{
    public class JiraProviderSettings
    {
        public string BaseUrl { get; set; }
        
        public string UserName { get; set; }
        
        public string Token { get; set; }

        public int BatchSize { get; set; }

        public List<string> ProjectNames { get; set; }
    }
}