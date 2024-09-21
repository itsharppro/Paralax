using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Paralax.Logging
{
    public class CorrelationContextLoggingMiddleware : IMiddleware
    {
        private readonly ILogger<CorrelationContextLoggingMiddleware> _logger;

        public CorrelationContextLoggingMiddleware(ILogger<CorrelationContextLoggingMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var headers = Activity.Current?.Baggage.ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, string>();
            using (_logger.BeginScope(headers))
            {
                await next(context);
            }
        }
    }
}
