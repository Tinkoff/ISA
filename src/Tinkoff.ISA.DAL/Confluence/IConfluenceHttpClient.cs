using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tinkoff.ISA.DAL.Confluence.Dtos;

namespace Tinkoff.ISA.DAL.Confluence
{
    public interface IConfluenceHttpClient
    {
        Task<ContentResponse> GetLatestPagesAsync(IEnumerable<string> spaceKeys, DateTime startDate);

        Task<ContentResponse> GetAllPageContentAsync(IEnumerable<string> spaceKeys);

        Task<ContentResponse> GetNextBatchAsync(string query);
    }
}
