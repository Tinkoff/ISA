using System;
using Microsoft.Extensions.DependencyInjection;
using Tinkoff.ISA.Core;
using AtlassianJira = Atlassian.Jira.Jira;

namespace Tinkoff.ISA.Providers.Jira
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddJiraProvider(this IServiceCollection serviceCollection, JiraProviderSettings settings)
        {
            serviceCollection.AddSingleton(settings);
            serviceCollection.AddSingleton(ProduceJiraKnowledgeProvider);
            
            return serviceCollection;
        }

        private static IKnowledgeProvider ProduceJiraKnowledgeProvider(IServiceProvider serviceProvider)
        {
            var settings = serviceProvider.GetService<JiraProviderSettings>();
            
            var jiraClient = AtlassianJira.CreateRestClient(settings.BaseUrl, settings.UserName, settings.Token);

            return new JiraKnowledgeProvider(jiraClient, settings);
        }
    }
}    
