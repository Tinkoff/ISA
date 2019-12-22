using System;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Tinkoff.ISA.AppLayer;
using Tinkoff.ISA.Core;

namespace Tinkoff.ISA.Scheduler.Jobs
{
    internal static class ServiceProviderExtensions
    {
        public static void AddJobs(this IServiceProvider services)
        {
            var recurringJobManager = services.GetRequiredService<IRecurringJobManager>();

            foreach (var knowledgeProvider in services.GetServices<IKnowledgeProvider>())
            {
                var providerTypeName = knowledgeProvider.GetType().Name;
                
                recurringJobManager.AddOrUpdate<ElasticIndexingJob>(
                    $"{nameof(ElasticIndexingJob)}_{providerTypeName}", 
                    job => job.Execute(), 
                    Cron.Minutely
                );
            }
        }
    }
}
