using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tinkoff.ISA.Core.Http;
using Tinkoff.ISA.DAL.Slack.Dtos;
using Tinkoff.ISA.Infrastructure.Exceptions;
using Tinkoff.ISA.Infrastructure.Settings;

namespace Tinkoff.ISA.DAL.Slack
{
    public class SlackHttpClient : ISlackHttpClient
    {
        private readonly IHttpClient _httpClient;
        private readonly JsonSerializerSettings _defaultSlackSerializerSettings;
        private readonly ILogger<SlackHttpClient> _logger;

        public SlackHttpClient(IHttpClient httpClient, ILogger<SlackHttpClient> logger,
            IOptions<SlackSettings> slackSettings)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpClient.Authorization = new AuthenticationHeaderValue("Bearer", slackSettings.Value.BotToken);
            _httpClient.BaseAddress = new Uri(slackSettings.Value.BaseAddress);
            _httpClient.Timeout = TimeSpan.FromMilliseconds(slackSettings.Value.HttpTimeoutMilliseconds);
            _defaultSlackSerializerSettings = SlackSerializerSettings.DefaultSettings;
        }

        public Task UpdateMessageAsync(string timestampOfMessageToUpdate, string channelId, string message = null,
            IList<AttachmentDto> attachments = null)
        {
            if (string.IsNullOrEmpty(channelId)) throw new ArgumentException(nameof(channelId));
            if (string.IsNullOrEmpty(timestampOfMessageToUpdate)) throw new ArgumentException(nameof(timestampOfMessageToUpdate));


            var sendMessageData = new Dictionary<string, string>
            {
                { "ts", timestampOfMessageToUpdate },
                { "channel", channelId },
                { "text", message},
                { "attachments", JsonConvert.SerializeObject(attachments, _defaultSlackSerializerSettings) }
            };

            return PostJsonAsync<SlackResponse>("chat.update", sendMessageData);
        }

        public Task SendMessageAsync(string channelId, string message, IList<AttachmentDto> attachments = null)
        {
            if (string.IsNullOrEmpty(channelId)) throw new ArgumentException(nameof(channelId));

            var sendMessageData = new Dictionary<string, string>
            {
                {"channel", channelId},
                {"text", message},
                {"attachments", JsonConvert.SerializeObject(attachments, _defaultSlackSerializerSettings)}
            };
            
            return PostJsonAsync<SlackResponse>("chat.postMessage", sendMessageData);
        }

        public Task OpenDialogAsync(DialogRequest dialogRequest)
        {
            return PostJsonAsync<SlackResponse>("dialog.open", dialogRequest);
        }

        public async Task<ChannelDto> OpenDirectMessageChannelAsync(string userId)
        {
            var response = await PostJsonAsync<OpenChannelResponse>("im.open", new { User = userId});
            return response.Channel;
        }

        private async Task<TResponse> PostJsonAsync<TResponse>(string method, object obj)
            where TResponse : SlackResponse
        {
            var apiBaseAddress = _httpClient.BaseAddress.ToString();
            _logger.LogInformation("HTTP-request (POST) to Slack | host: {SlackHost} | uri: {SlackRequestUrl}",
                apiBaseAddress, method);

            var objJson = JsonConvert.SerializeObject(obj, _defaultSlackSerializerSettings);
            var content = new StringContent(objJson, Encoding.UTF8, MediaTypeNames.Application.Json);

            var httpResponse = await _httpClient.PostAsync(method, content);

            if (httpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new ExternalApiInvocationException(
                    _httpClient.BaseAddress.ToString(), method,
                    $"Expected status code 200 but received {httpResponse.StatusCode}");
            }

            var responseContent = await httpResponse.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<TResponse>(responseContent, _defaultSlackSerializerSettings);
            
            if (!response.Ok)
                throw new SlackException($"Error running method {method}: {response.Error}");
            return response;
        }
    }
}
