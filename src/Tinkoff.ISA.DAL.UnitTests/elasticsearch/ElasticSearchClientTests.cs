using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using Nest;
using Tinkoff.ISA.DAL.Elasticsearch;
using Tinkoff.ISA.DAL.Elasticsearch.Client;
using Tinkoff.ISA.DAL.Elasticsearch.Request;
using Tinkoff.ISA.Domain.Search;
using Tinkoff.ISA.Infrastructure.Settings;
using Xunit;

namespace Tinkoff.ISA.DAL.UnitTests.elasticsearch
{
    public class ElasticSearchClientTests
    {
        private const string Text = "test";
        private readonly Mock<IElasticClientWrapper> _elasticClientWrapperMock;
        private readonly IElasticSearchClient _client;
        private readonly Mock<ISearchResponse<SearchableQuestion>> _searchResponseMock;

        public ElasticSearchClientTests()
        {
            var elasticSettingsOptionsMock = new Mock<IOptions<ElasticSearchSettings>>();
            elasticSettingsOptionsMock
                .SetupGet(m => m.Value)
                .Returns(() => new ElasticSearchSettings()
                {
                    Url = "http://localhost:9200"
                });


            _elasticClientWrapperMock = new Mock<IElasticClientWrapper>();
            _searchResponseMock = new Mock<ISearchResponse<SearchableQuestion>>();

            _searchResponseMock.Setup(x => x.Documents)
                .Returns(new List<SearchableQuestion>
                {
                    new SearchableQuestion {Text = Text, Id = Guid.NewGuid().ToString()}
                });
            _searchResponseMock.SetupGet(x => x.ApiCall.HttpStatusCode)
                .Returns((int)HttpStatusCode.OK);

            _elasticClientWrapperMock.Setup(x =>
                x.SearchAsync(
                    It.IsAny<Func<SearchDescriptor<SearchableQuestion>, ISearchRequest>>()))
                .ReturnsAsync(_searchResponseMock.Object);

            _client = new ElasticSearchClient(_elasticClientWrapperMock.Object);
        }

        [Fact]
        public async void SearchAsync_CorrectParameters_ShouldCallElasticClientSearchAsync()
        {
            //arrange
            var request = new QuestionElasticSearchRequest
            {
                Text = "test"
            };

            _searchResponseMock.SetupGet(x => x.ApiCall.HttpStatusCode)
                .Returns((int) HttpStatusCode.OK);
            //act 
            await _client.SearchAsync<SearchableQuestion>(request,
                f => f.Match(mqd => mqd.Query("test text").Fuzziness(Fuzziness.Auto)));

            //assert
            _elasticClientWrapperMock.Verify(
                v => v.SearchAsync(
                    It.IsAny<Func<SearchDescriptor<SearchableQuestion>, ISearchRequest>>()), Times.Once);
        }

        [Fact]
        public async void SearchAsync_StatusCodeIsNotOK_ElasticException()
        {
            //arrange
            _searchResponseMock.SetupGet(x => x.ApiCall.HttpStatusCode)
                .Returns((int)HttpStatusCode.NoContent);

            var request = new QuestionElasticSearchRequest
            {
                Text = "test"
            };

            //act, assert
            await Assert.ThrowsAsync<ElasticException>(() =>
                _client.SearchAsync<SearchableQuestion>(request,
                    f => f.Match(mqd => mqd.Query("test text").Fuzziness(Fuzziness.Auto))));
        }

        [Fact]
        public async void UpsertAsync_CorrectRequest_ShouldCallElasticClientUpdateAsync()
        {
            // Arrange
            var request = new ConfluenceElasticUpsertRequest
            {
                Entities = new List<SearchableConfluence>()
            };

            _elasticClientWrapperMock.Setup(m => m.BulkAsync(It.IsAny<Func<BulkDescriptor, IBulkRequest>>()))
                .Returns(Task.CompletedTask);

            // Act 
            await _client.UpsertManyAsync(request);

            // Assert
            _elasticClientWrapperMock.Verify(
                m => m.BulkAsync(It.IsAny<Func<BulkDescriptor, IBulkRequest>>()), Times.Once);
            _elasticClientWrapperMock.VerifyNoOtherCalls();
        }
    }
}
