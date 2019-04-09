using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Atlassian.Jira;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tinkoff.ISA.DAL.Elasticsearch.Client;
using Tinkoff.ISA.DAL.Elasticsearch.Request;
using Tinkoff.ISA.DAL.Jira;
using Tinkoff.ISA.DAL.Storage.Dao.Application;
using Tinkoff.ISA.Domain.Search;
using Tinkoff.ISA.Infrastructure.Settings;

namespace Tinkoff.ISA.AppLayer.Jobs
{
    public class JiraJob : IJob
    {
        private readonly ILogger<JiraJob> _logger;
        private readonly IJiraClient _jiraClient;
        private readonly IElasticsearchClient _elasticsearchClient;
        private readonly IApplicationPropertyDao _applicationPropertyDao;
        private readonly IOptions<JiraSettings> _settings;
        private const string TimeFormat = "yyyy/MM/dd HH:mm";
        
        public JiraJob(
            ILogger<JiraJob> logger,
            IElasticsearchClient elasticsearchClient,
            IApplicationPropertyDao applicationPropertyDao,
            IJiraClient jiraClient,
            IOptions<JiraSettings> settings)
        {
            _logger = logger;
            _elasticsearchClient = elasticsearchClient;
            _applicationPropertyDao = applicationPropertyDao;
            _jiraClient = jiraClient;
            _settings = settings;
        }

        public async Task StartJob()
        {
            var appProperties = await _applicationPropertyDao.GetAsync();
            var loadFromDate = appProperties?.JiraJobLastUpdate ?? DateTime.MinValue;
            var loadFromDateLocal = loadFromDate.ToLocalTime();
            var isAllIssuesUpdated = false;
            var startAt = 0;

            while (!isAllIssuesUpdated)
            {
                var jiraResponse = await _jiraClient.GetLatestIssuesAsync(_settings.Value.ProjectNames, loadFromDateLocal, _settings.Value.BatchSize, startAt);

                await UploadIssuesBatch(jiraResponse);

                var lastIssueDateTimeUpdated = jiraResponse.Last().Updated;
                if (lastIssueDateTimeUpdated.HasValue)
                {
                    var loadFromDateTimeLocalFormat = loadFromDateLocal.ToString(TimeFormat, CultureInfo.InvariantCulture);
                    var lastIssueDateTimeUpdatedFormat = lastIssueDateTimeUpdated.Value.ToString(TimeFormat, CultureInfo.InvariantCulture);

                    if (loadFromDateTimeLocalFormat == lastIssueDateTimeUpdatedFormat)
                    {
                        startAt += _settings.Value.BatchSize;
                    }
                    else
                    {
                        startAt = 0;
                        loadFromDateLocal = lastIssueDateTimeUpdated.Value;
                    }

                    await _applicationPropertyDao.UpsertPropertyAsync(p => p.JiraJobLastUpdate, loadFromDateLocal.ToUniversalTime());
                }

                isAllIssuesUpdated = jiraResponse.TotalItems == jiraResponse.Count();
            }
        }

        private async Task UploadIssuesBatch(IPagedQueryResult<Issue> jiraResponse)
        {
            var elasticRequest = new JiraElasticUpsertRequest
            {
                Entities = CreateUpsertData(jiraResponse)
            };

            await _elasticsearchClient.UpsertManyAsync(elasticRequest);

            var titlesWithIds = jiraResponse.Select(s => $" {s.Summary}-{s.Key}");
            _logger.LogInformation("JiraJob | {BatchUploadDate} | remains to load: {countOfRemaining} | batch loaded: [{uploadedDataIdentifiers}]", 
                DateTime.Now,
                jiraResponse.TotalItems,
                string.Join(',', titlesWithIds));
        }

        private List<SearchableJira> CreateUpsertData(IEnumerable<Issue> rawContent)
        {
            return rawContent.Select(s => new SearchableJira
            {
                Id = s.JiraIdentifier,
                Title = s.Summary,
                Link = $"{_settings.Value.BaseAddress}browse/{s.Key}",
                Text = s.Description
            }).ToList();
        }
    }
}
