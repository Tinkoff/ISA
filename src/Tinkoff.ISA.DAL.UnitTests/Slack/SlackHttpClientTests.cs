using System;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Tinkoff.ISA.Core.Http;
using Tinkoff.ISA.DAL.Slack;
using Tinkoff.ISA.Infrastructure.Exceptions;
using Tinkoff.ISA.Infrastructure.Settings;
using Xunit;

namespace Tinkoff.ISA.DAL.UnitTests.Slack
{
    public class SlackHttpClientTests
    {
        private const string ChannelId = "TEST_CHANNEL";
        private const string Message = "TEST_MESSAGE";
        private const string Ts = "TIME";
        private readonly Mock<IHttpClient> _httpClientMock;
        private readonly ISlackHttpClient _slackHttpClient;

        public SlackHttpClientTests()
        {

            var slackSettingOptionsMock = new Mock<IOptions<SlackSettings>>();
            slackSettingOptionsMock
                .SetupGet(m => m.Value)
                .Returns(() => new SlackSettings()
                {
                    BotToken = "TEST_TOKEN",
                    BaseAddress = "https://slack.com",
                    HttpTimeoutMilliseconds = 5000
                });

            _httpClientMock = new Mock<IHttpClient>();
            _httpClientMock.SetupAllProperties();
            var logger = new Mock<ILogger<SlackHttpClient>>();
            _slackHttpClient = new SlackHttpClient(_httpClientMock.Object, logger.Object, slackSettingOptionsMock.Object);
        }

        [Fact]
        public async void SendMessageAsync_CorrectParametres_ShouldCallPostAsync()
        {
            //arange 
            _httpClientMock
                .Setup(m => m.PostAsync(It.IsAny<string>(), It.IsAny<StringContent>()))
                .ReturnsAsync(new HttpResponseMessage()
                {
                    Content = new StringContent("{\"ok\":\"true\"}")
                });
            //act
            await _slackHttpClient.SendMessageAsync(ChannelId, Message);
            
            //assert
            _httpClientMock.Verify(
                m => m.PostAsync(It.Is<string>(x => x == "chat.postMessage"),
                    It.IsAny<StringContent>()), Times.Once);
        }

        [Fact]
        public async void SendMessageAsync_ChannelAreInvalid_ArgumentException()
        {
            //act, assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _slackHttpClient.SendMessageAsync(null, Message));
        }
        
        [Fact]
        public async void SendMessageAsync_StatusCodeIsNotOK_SlackException()
        {
            //arange 
            _httpClientMock
                .Setup(m => m.PostAsync(It.IsAny<string>(), It.IsAny<StringContent>()))
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("{\"ok\":\"true\"}")
                });

            //act, assert
            await Assert.ThrowsAsync<ExternalApiInvocationException>(() =>
                _slackHttpClient.SendMessageAsync(ChannelId, Message));
        }

        [Fact]
        public async void SendMessageAsync_ResponseContentOkFieldIsNotTrue_SlackException()
        {
            //arange 
            _httpClientMock
                .Setup(m => m.PostAsync(It.IsAny<string>(), It.IsAny<StringContent>()))
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"ok\":\"false\"}")
                });

            //act, assert
            await Assert.ThrowsAsync<SlackException>(() =>
                _slackHttpClient.SendMessageAsync(ChannelId, Message));
        }


        [Fact]
        public async void UpdateMessageAsync_CorrectParametres_ShouldCallPostAsync()
        {
            //arange 
            _httpClientMock
                .Setup(m => m.PostAsync(It.IsAny<string>(), It.IsAny<StringContent>()))
                .ReturnsAsync(new HttpResponseMessage()
                {
                    Content = new StringContent("{\"ok\":\"true\"}")
                });
            //act
            await _slackHttpClient.UpdateMessageAsync(Ts, ChannelId, Message);

            //assert
            _httpClientMock.Verify(
                m => m.PostAsync(It.Is<string>(x => x == "chat.update"),
                    It.IsAny<StringContent>()), Times.Once);
        }

        [Fact]
        public async void UpdateMessageAsync_StatusCodeIsNotOK_SlackException()
        {
            //arange 
            _httpClientMock
                .Setup(m => m.PostAsync(It.IsAny<string>(), It.IsAny<StringContent>()))
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("{\"ok\":\"true\"}")
                });

            //act, assert
            await Assert.ThrowsAsync<ExternalApiInvocationException>(() =>
                _slackHttpClient.UpdateMessageAsync(Ts, ChannelId, Message));
        }

        [Fact]
        public async void UpdateMessageAsync_ResponseContentOkFieldIsNotTrue_SlackException()
        {
            //arange 
            _httpClientMock
                .Setup(m => m.PostAsync(It.IsAny<string>(), It.IsAny<StringContent>()))
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"ok\":\"false\"}")
                });

            //act, assert
            await Assert.ThrowsAsync<SlackException>(() =>
                _slackHttpClient.UpdateMessageAsync(Ts, ChannelId, Message));
        }

        [Fact]
        public async void UpdateMessageAsync_ChannelAreInvalid_ArgumentException()
        {
            //act, assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _slackHttpClient.UpdateMessageAsync(Ts, null, Message));
        }

        [Fact]
        public async void UpdateMessageAsync_TsAreInvalid_ArgumentException()
        {
            //act, assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _slackHttpClient.UpdateMessageAsync(null, ChannelId, Message));
        }
    }
}