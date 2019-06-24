using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tinkoff.ISA.Providers.Jira
{
    public class JiraProviderSettings
    {
        [Required]
        public string BaseUrl { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Token { get; set; }

        public int BatchSize { get; set; }

        public List<string> ProjectNames { get; set; }
    }
}