using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Paralax.Metrics.Prometheus.Core
{
    internal sealed class PrometheusMiddleware : IMiddleware
    {
        private readonly ISet<string> _allowedHosts;
        private readonly string _endpoint;
        private readonly string _apiKey;

        public PrometheusMiddleware(PrometheusOptions options)
        {
            _allowedHosts = new HashSet<string>(options.AllowedHosts ?? Array.Empty<string>());
            _endpoint = string.IsNullOrWhiteSpace(options.Endpoint) ? "/metrics" :
                options.Endpoint.StartsWith("/") ? options.Endpoint : $"/{options.Endpoint}";
            _apiKey = options.ApiKey;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var request = context.Request;

            if (request.Path != _endpoint)
            {
                await next(context);
                return;
            }

            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                await next(context);
                return;
            }

            if (request.Query.TryGetValue("apiKey", out var apiKey) && apiKey == _apiKey)
            {
                await next(context);
                return;
            }

            var host = request.Host.Host;
            if (_allowedHosts.Contains(host))
            {
                await next(context);
                return;
            }

            if (request.Headers.TryGetValue("x-forwarded-for", out var forwardedFor) && _allowedHosts.Contains(forwardedFor))
            {
                await next(context);
                return;
            }

            context.Response.StatusCode = 404;
        }
    }
}
