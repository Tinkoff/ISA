using Microsoft.AspNetCore.Builder;
using Tinkoff.ISA.Infrastructure.Middleware;

namespace Tinkoff.ISA.Infrastructure.Extensions
{
    public static class LogExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseLogException(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LogExceptionMiddleware>();
        }
    }
}
