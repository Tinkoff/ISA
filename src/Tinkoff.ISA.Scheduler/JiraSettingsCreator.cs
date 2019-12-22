using System;
using Microsoft.Extensions.Configuration;
using Tinkoff.ISA.Providers.Jira;

namespace Tinkoff.ISA.Scheduler
{
    internal static class JiraSettingsCreator
    {
        public static JiraProviderSettings CreateProviderSettings(IConfiguration configuration)
        {
            const string sectionName = "JiraSettings";
            
            var jiraSection = configuration.GetSection(sectionName);
            
            if (!jiraSection.Exists())
            {
                throw new InvalidOperationException($"\"{sectionName}\" section doesn't exist.");
            }
            
            return jiraSection.Get<JiraProviderSettings>();
        }
    }
}
