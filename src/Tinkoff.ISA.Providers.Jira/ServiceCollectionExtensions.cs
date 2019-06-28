using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tinkoff.ISA.Core;
using AtlassianJira = Atlassian.Jira.Jira;

namespace Tinkoff.ISA.Providers.Jira
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddJiraProvider(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            var settings = CreateJiraProviderSettings(configuration);
            
            serviceCollection.AddSingleton(settings);
            serviceCollection.AddSingleton(ProduceJiraKnowledgeProvider);
            
            return serviceCollection;
        }

        private static JiraProviderSettings CreateJiraProviderSettings(IConfiguration configuration)
        {
            const string sectionName = "JiraSettings";
            
            var jiraSection = configuration.GetSection(sectionName);
            if (!jiraSection.Exists())
            {
                throw new InvalidOperationException($"\"{sectionName}\" section doesn't exist.");
            }
            
            var settings = new JiraProviderSettings();
            configuration.Bind(sectionName, settings);
            
            return settings;
        }

        private static IKnowledgeProvider ProduceJiraKnowledgeProvider(IServiceProvider serviceProvider)
        {
            var settings = serviceProvider.GetService<JiraProviderSettings>();
            
            var jiraClient = AtlassianJira.CreateRestClient(settings.BaseUrl, settings.UserName, settings.Token);

            return new JiraKnowledgeProvider(jiraClient, settings);
        }
    }
}    
