using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Tinkoff.ISA.Infrastructure.Middleware
{
    public class LogExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LogExceptionMiddleware> _logger;

        public LogExceptionMiddleware(RequestDelegate next, ILogger<LogExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        }
    }
}
