using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Nest;
using Tinkoff.ISA.DAL.Elasticsearch.Client;
using Tinkoff.ISA.DAL.Elasticsearch.Request;
using Tinkoff.ISA.Domain.Search;
using Tinkoff.ISA.Infrastructure.Settings;

namespace Tinkoff.ISA.DAL.Elasticsearch.Services
{
    internal class ElasticSearchService : IElasticSearchService
    {
        private const string Stemming = "stemming";
        private const string Original = "original";
        private const string Shingles = "shingles";
        
        private readonly IElasticSearchClient _elasticSearchClient;
        private readonly IOptions<ElasticSearchSettings> _elasticsearchSettings;

        public ElasticSearchService(IElasticSearchClient elasticSearchClient, IOptions<ElasticSearchSettings> elasticsearchSettings)
        {
            _elasticSearchClient = elasticSearchClient;
            _elasticsearchSettings = elasticsearchSettings;
        }

        public Task<IList<TResponse>> SearchAsync<TResponse>(ElasticSearchRequest request) 
            where TResponse : SearchableText
        {
            request.SearchParams = GetIndexSearchParams(request.Index);

            return _elasticSearchClient.SearchAsync<TResponse>(request,
                q => q.Bool(s => s
                    .Should(
                        qs => qs.MultiMatch(c => c
                            .Query(request.Text)
                            .MinimumShouldMatch(request.SearchParams.MinShouldMatchPercentage)
                            .Type(TextQueryType.MostFields)
                            .Fields(f => f.Field(p => p.Text))),
                        qs => qs.MultiMatch(c => c
                            .Query(request.Text)
                            .MinimumShouldMatch(request.SearchParams.MinShouldMatchPercentage)
                            .Fuzziness(Fuzziness.Auto)
                            .PrefixLength(request.SearchParams.PrefixLength)
                            .MaxExpansions(request.SearchParams.MaxExpansions)
                            .Type(TextQueryType.MostFields)
                            .Fields(f => f
                                .Field(p => p.Text.Suffix(Stemming))
                                .Field(p => p.Text.Suffix(Original))
                                .Field(p => p.Text.Suffix(Shingles))))
                    )));
        }

        public Task<IList<TResponse>> SearchWithTitleAsync<TResponse>(ElasticSearchRequest request) 
            where TResponse : SearchableWithTitle
        {
            request.SearchParams = GetIndexSearchParams(request.Index);

            return _elasticSearchClient.SearchAsync<TResponse>(request,
                q => q.Bool(s => s
                    .Should(
                        qs => qs.MultiMatch(c => c
                            .Query(request.Text)
                            .MinimumShouldMatch(request.SearchParams.MinShouldMatchPercentage)
                            .Type(TextQueryType.MostFields)
                            .Fields(f => f
                                .Field(p => p.Title)
                                .Field(p => p.Title.Suffix(Stemming))
                                .Field(p => p.Title.Suffix(Original))
                                .Field(p => p.Title.Suffix(Shingles))
                                .Field(p => p.Text))
                            .Boost(2)), 
                        qs => qs.MultiMatch(c => c
                            .Query(request.Text)
                            .MinimumShouldMatch(request.SearchParams.MinShouldMatchPercentage)
                            .Fuzziness(Fuzziness.Auto)
                            .PrefixLength(request.SearchParams.PrefixLength)
                            .MaxExpansions(request.SearchParams.MaxExpansions)
                            .Type(TextQueryType.MostFields)
                            .Fields(f => f 
                                .Field(p => p.Title.Suffix(Stemming))
                                .Field(p => p.Title.Suffix(Original))
                                .Field(p => p.Title.Suffix(Shingles))))
                    )));
        }
        
        private IndexSearchParams GetIndexSearchParams(string indexName)
        {
            if (_elasticsearchSettings.Value.IndexSearchParams.TryGetValue(indexName, out var searchParams))
            {
                return searchParams;
            }

            throw new IndexOutOfRangeException($"Found no search settings for {indexName} index");
        }
    }
}
