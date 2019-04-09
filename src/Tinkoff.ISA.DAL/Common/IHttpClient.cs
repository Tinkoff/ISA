using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Tinkoff.ISA.DAL.Common
{
    public interface IHttpClient : IDisposable
    {
        Uri BaseAddress { get; set; }

        TimeSpan Timeout { get; set; }

        AuthenticationHeaderValue Authorization { get; set; }

        Task<HttpResponseMessage> PostAsync(string requestUri, StringContent content);

        Task<HttpResponseMessage> GetAsync(string requestUri);
    }
}