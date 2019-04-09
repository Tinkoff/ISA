using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Tinkoff.ISA.DAL.Common;
using Tinkoff.ISA.DAL.Confluence;
using Tinkoff.ISA.DAL.Confluence.Dtos;
using Tinkoff.ISA.Infrastructure.Exceptions;
using Tinkoff.ISA.Infrastructure.Settings;
using Xunit;

namespace Tinkoff.ISA.DAL.UnitTests.Atlassian
{
    public class ConfluenceHttpClientTests
    {
        private const string CqlDateFormat = "yyyy'/'MM'/'dd";
        private readonly Mock<IHttpClient> _httpClientMock;
        private readonly ConfluenceHttpClient _client;

        public ConfluenceHttpClientTests()
        {
            var atlassianOptions = new Mock<IOptions<ConfluenceSettings>>();
            atlassianOptions
                .SetupGet(m => m.Value)
                .Returns(() => new ConfluenceSettings
                {
                    BaseAddress = "https://your_wiki_address",
                    BatchSize = 2,
                    HttpTimeoutMilliseconds = 1000,
                    User = "vasya",
                    Password = "vasya_the_best"
                });

            _httpClientMock = new Mock<IHttpClient>();
            _httpClientMock.SetupAllProperties();
            var logger = new Mock<ILogger<ConfluenceHttpClient>>();
            _client = new ConfluenceHttpClient(_httpClientMock.Object, atlassianOptions.Object, logger.Object);
        }

        [Fact]
        public async Task GetLatestPageContent_ResponseCodeIsNotOk_ExternalApiInvocationException()
        {
            // Arrange
            _httpClientMock.Setup(m => m.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            // Act-Assert
            await Assert.ThrowsAsync<ExternalApiInvocationException>(() =>
                _client.GetLatestPagesAsync(new []{"IH"}, DateTime.Today));
        }

        [Fact]
        public async Task GetLatestPageContent_ResponseBodyIsEmpty_ExternalApiInvocationException()
        {
            // Arrange
            _httpClientMock.Setup(m => m.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("")
                });

            // Act-Assert
            await Assert.ThrowsAsync<ExternalApiInvocationException>(() =>
                _client.GetLatestPagesAsync(new []{"IH"}, DateTime.Today));
        }

        [Fact]
        public async Task GetAllPageContent_RepsonseCodeIsNotOk_ExternalApiInvocationException()
        {
            // Arrange
            _httpClientMock.Setup(m => m.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            // Act-Assert
            await Assert.ThrowsAsync<ExternalApiInvocationException>(() =>
                _client.GetAllPageContentAsync(new []{"IH"}));
        }

        [Fact]
        public async Task GetAllPageContent_ResponseBodyIsEmpty_ExternalApiInvocationException()
        {
            // Arrange
            _httpClientMock.Setup(m => m.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("")
                });

            // Act-Assert
            await Assert.ThrowsAsync<ExternalApiInvocationException>(() =>
                _client.GetAllPageContentAsync(new []{"IH"}));
        }

        [Fact]
        public async Task GetPageContent_ResultsTakeTwoPages_ShouldReturnAllAndSpecifyDateAlsoSpecifyNextUrl()
        {
            // Arrange
            var date = DateTime.MinValue;
            var expectedEncodedDate = WebUtility.UrlEncode(date.ToString(CqlDateFormat));
            var pageContent = new List<ContentDto>
            {
                new ContentDto
                {
                    Id = "id1",
                    Title = "title1",
                    Body = new ContentBodyDto
                    {
                        View = new ViewRepresentationDto
                        {
                            Value = "body goes here 1"
                        }
                    }
                },
                new ContentDto
                {
                    Id = "id2",
                    Title = "title2",
                    Body = new ContentBodyDto
                    {
                        View = new ViewRepresentationDto
                        {
                            Value = "body goes here 2"
                        }
                    }
                }
            };

            var firstPage = new ContentResponse
            {
                Links = new LinksDto { Next = "next url request"},
                Results = pageContent
            };

            _httpClientMock.Setup(m => m.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(firstPage))
                });

            // Act
            var result = await _client.GetLatestPagesAsync(new []{"IH"}, date);
            
            // Assert
            Assert.Equal(result.Results.Count, pageContent.Count);
            _httpClientMock.Verify(m => m.GetAsync(It.Is<string>(q => q.Contains(expectedEncodedDate))),
                Times.Once);
        }
    }
}
