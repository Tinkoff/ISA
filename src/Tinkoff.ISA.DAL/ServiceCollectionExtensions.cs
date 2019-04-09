using Microsoft.Extensions.DependencyInjection;
using Tinkoff.ISA.DAL.Common;
using Tinkoff.ISA.DAL.Confluence;
using Tinkoff.ISA.DAL.Elasticsearch.Client;
using Tinkoff.ISA.DAL.Elasticsearch.Services;
using Tinkoff.ISA.DAL.Jira;
using Tinkoff.ISA.DAL.Slack;
using Tinkoff.ISA.DAL.Storage.Common;
using Tinkoff.ISA.DAL.Storage.Dao.Application;
using Tinkoff.ISA.DAL.Storage.Dao.Questions;

namespace Tinkoff.ISA.DAL
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDalDependencies(this IServiceCollection services)
        {
            services.AddSingleton(typeof(IRepository<,>), typeof(MongoRepository<,>));
            services.AddSingleton<IQuestionDao, QuestionDao>();
            services.AddSingleton<IApplicationPropertyDao, ApplicationPropertyDao>();
            services.AddSingleton<ISlackHttpClient, SlackHttpClient>();
            services.AddSingleton<IHttpClient, HttpClientWrapper>();
            services.AddSingleton<IElasticClientWrapper, ElasticClientWrapper>();
            services.AddSingleton<IElasticsearchClient, ElasticsearchClient>();
            services.AddSingleton<IConfluenceHttpClient, ConfluenceHttpClient>();
            services.AddSingleton<IElasticSearchService, ElasticSearchService>();
            services.AddSingleton<IJiraClient, JiraClient>();
            services.AddSingleton<IJiraClientWrapper, JiraClientWrapper>();
            return services;
        }
    }
}
