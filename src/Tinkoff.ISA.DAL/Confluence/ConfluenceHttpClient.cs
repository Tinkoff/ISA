using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Tinkoff.ISA.DAL.Common;
using Tinkoff.ISA.DAL.Confluence.Dtos;
using Tinkoff.ISA.Infrastructure.Exceptions;
using Tinkoff.ISA.Infrastructure.Settings;

namespace Tinkoff.ISA.DAL.Confluence
{
    internal class ConfluenceHttpClient : IConfluenceHttpClient
    {
        private const string CqlQueryDateFormat = "yyyy/MM/dd HH:mm";
        private const string SearchMethod = "/rest/api/content/search";
        private readonly int _batchSize;
        private readonly IHttpClient _httpClient;
        private readonly ILogger<ConfluenceHttpClient> _logger;

        public ConfluenceHttpClient(IHttpClient httpClient, IOptions<ConfluenceSettings> settings, ILogger<ConfluenceHttpClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            // https://developer.atlassian.com/cloud/confluence/authentication-for-apps/
            var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(settings.Value.User + ":" + settings.Value.Password));
            
            _httpClient.Authorization = new AuthenticationHeaderValue("Basic", encoded);
            _httpClient.BaseAddress = new Uri(settings.Value.BaseAddress);
            _httpClient.Timeout = TimeSpan.FromMilliseconds(settings.Value.HttpTimeoutMilliseconds);
            _batchSize = settings.Value.BatchSize;
        }

        public Task<ContentResponse> GetLatestPagesAsync(IEnumerable<string> spaceKeys, DateTime startDate)
        {
            if (spaceKeys == null) throw new ArgumentNullException();

            var query = CreateQueryString(spaceKeys, startDate);
            return GetPageContentAsync(query);
        }

        public Task<ContentResponse> GetAllPageContentAsync(IEnumerable<string> spaceKeys)
        {
            if (spaceKeys == null) throw new ArgumentNullException();

            var query = CreateQueryString(spaceKeys, DateTime.MinValue);
            return GetPageContentAsync(query);
        }

        public Task<ContentResponse> GetNextBatchAsync(string query)
        {
            if (string.IsNullOrEmpty(query)) throw new ArgumentException();

            return GetPageContentAsync(query);
        }

        private async Task<ContentResponse> GetPageContentAsync(string queryString)
        {
            var apiBaseAddress = _httpClient.BaseAddress.ToString();
            _logger.LogInformation("HTTP-request (GET) to Confluence | host: {ConfluenceHost} | uri: {ConfluenceRequestUrl}",
                apiBaseAddress, queryString);

            var response = await _httpClient.GetAsync(queryString);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new ExternalApiInvocationException(
                    _httpClient.BaseAddress.ToString(), SearchMethod,
                    $"HTTP-200 was expected, but was received {response.StatusCode}");
            }

            var body = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(body))
                throw new ExternalApiInvocationException(_httpClient.BaseAddress.ToString(), SearchMethod, "Empty response is received");

            var contentResponse = JsonConvert.DeserializeObject<ContentResponse>(body);
            return contentResponse;
        }

        private string CreateQueryString(IEnumerable<string> spaceKeys, DateTime startDate)
        {
            var date = startDate.ToString(CqlQueryDateFormat, CultureInfo.InvariantCulture);
            var cqlQuery =
                $"((lastModified >= \"{date}\" or created >= \"{date}\") and type=page " +
                $"and space.key in ({string.Join(", ", spaceKeys)})) order by created asc, lastModified asc";

            var parameters = new Dictionary<string, string>
            {
                {"cql", cqlQuery},
                {"limit", _batchSize.ToString()},
                {"expand", "body.view,version"}
            };

            return QueryHelpers.AddQueryString(SearchMethod, parameters);
        }
    }
}
