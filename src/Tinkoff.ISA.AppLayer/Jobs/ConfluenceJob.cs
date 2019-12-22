using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tinkoff.ISA.DAL.Confluence;
using Tinkoff.ISA.DAL.Confluence.Dtos;
using Tinkoff.ISA.DAL.Elasticsearch.Client;
using Tinkoff.ISA.DAL.Elasticsearch.Request;
using Tinkoff.ISA.DAL.Storage.Dao.Application;
using Tinkoff.ISA.Domain.Search;
using Tinkoff.ISA.Infrastructure.Settings;

namespace Tinkoff.ISA.AppLayer.Jobs
{
    internal class ConfluenceJob : IJob
    {
        private readonly IConfluenceHttpClient _confluenceHttpClient;
        private readonly IElasticSearchClient _elasticSearchClient;
        private readonly IApplicationPropertyDao _applicationPropertyDao;
        private readonly IOptions<ConfluenceSettings> _settings;
        private readonly ILogger<ConfluenceJob> _logger;

        public ConfluenceJob(IConfluenceHttpClient confluenceHttpClient,
            IElasticSearchClient elasticSearchClient,
            IApplicationPropertyDao applicationPropertyDao,
            IOptions<ConfluenceSettings> settings,
            ILogger<ConfluenceJob> logger)
        {
            _confluenceHttpClient = confluenceHttpClient;
            _elasticSearchClient = elasticSearchClient;
            _applicationPropertyDao = applicationPropertyDao;
            _settings = settings;
            _logger = logger;
        }

        public async Task StartJob()
        {
            var appProperties = await _applicationPropertyDao.GetAsync();
            var startDate = appProperties?.ConfluenceJobLastUpdate.ToLocalTime() ?? DateTime.MinValue;

            var response = await _confluenceHttpClient.GetLatestPagesAsync(_settings.Value.SpaceKeys, startDate);
            UploadBatch(response);

            while (response.Links?.Next != null)
            {
                var nextBatchQuery = response.Links.Next;
                response = await _confluenceHttpClient.GetNextBatchAsync(nextBatchQuery);
                UploadBatch(response);
            }
        }

        private static string CleanPage(string pageText)
        {
            const string tagsPattern = "<[^>]*>";
            const string extraSpacesPattern = "[ ]{2,}";
            
            var decodedHtml = WebUtility.HtmlDecode(pageText);
            var tagsRemoved = Regex.Replace(decodedHtml, tagsPattern, " ");
            
            return Regex.Replace(tagsRemoved, extraSpacesPattern, " ").Trim();
        }

        private static DateTime GetLastDate(ContentResponse response)
        {
            var latestPage = response.Results.Last();  
            return latestPage.Version.When.ToUniversalTime();
        }

        private void LogUploadedBatch(ConfluenceElasticUpsertRequest request)
        {
            var titlesAndIds = request.Entities.Select(e => $"{{id:{e.Id} title:{e.Title}}}");
            
            _logger.LogInformation("{JobName} | {BatchUploadDate} | batch loaded: [{uploadedDataIdentifiers}]",
                nameof(ConfluenceJob), DateTime.Now,
                string.Join(',', titlesAndIds));
        }
        
        private async void UploadBatch(ContentResponse response)
        {
            if (response.Results.Count == 0) return;

            var request = new ConfluenceElasticUpsertRequest
            {
                Entities = CreateSearchablePages(response.Results)
            };
            
            await _elasticSearchClient.UpsertManyAsync(request);
            
            LogUploadedBatch(request);

            await _applicationPropertyDao.UpsertPropertyAsync(p => p.ConfluenceJobLastUpdate,
                GetLastDate(response));
        }

        private List<SearchableConfluence> CreateSearchablePages(IEnumerable<ContentDto> rawContent)
        {
            return rawContent.Select(c => new SearchableConfluence
            {
                Id = c.Id,
                Title = c.Title,
                Link = _settings.Value.BaseAddress + c.Links.Webui,
                Text = CleanPage(c.Body.View.Value)
            }).ToList();
        }
    }
}
