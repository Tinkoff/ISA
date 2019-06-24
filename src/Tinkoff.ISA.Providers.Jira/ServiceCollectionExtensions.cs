using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Tinkoff.ISA.Core;
using static Atlassian.Jira.Jira;

namespace Tinkoff.ISA.Providers.Jira
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddJiraProvider(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            var jiraSection = GetJiraSection(configuration);
            
            serviceCollection.AddOptions<JiraProviderSettings>()
                .Bind(jiraSection)
                .ValidateDataAnnotations();

            serviceCollection.AddSingleton(ProduceJiraKnowledgeProvider);
            
            return serviceCollection;
        }

        private static IConfigurationSection GetJiraSection(IConfiguration configuration)
        {
            const string sectionName = "JiraSettings";
            
            var jiraSection = configuration.GetSection(sectionName);

            if (!jiraSection.Exists())
            {
                throw new InvalidOperationException($"\"{sectionName}\" section doesn't exist.");
            }

            return jiraSection;
        }

        private static IKnowledgeProvider ProduceJiraKnowledgeProvider(IServiceProvider serviceProvider)
        {
            var options = serviceProvider.GetService<IOptions<JiraProviderSettings>>();
            
            var settings = options.Value;
            var jiraClient = CreateRestClient(settings.BaseUrl, settings.UserName, settings.Token);
                    
            return new JiraKnowledgeProvider(jiraClient, options);
        }
    }
}    