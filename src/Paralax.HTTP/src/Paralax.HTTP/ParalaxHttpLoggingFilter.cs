using System;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Paralax.HTTP
{
    internal sealed class ParalaxHttpLoggingFilter : IHttpMessageHandlerBuilderFilter
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly HttpClientOptions _options;

        public ParalaxHttpLoggingFilter(ILoggerFactory loggerFactory, HttpClientOptions options)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
        {
            if (next is null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            return builder =>
            {
                next(builder);

                var logger = _loggerFactory.CreateLogger($"System.Net.Http.HttpClient.{builder.Name}.LogicalHandler");

                builder.AdditionalHandlers.Insert(0, new ParalaxLoggingHttpMessageScopeHandler(logger, _options));
            };
        }
    }
}
