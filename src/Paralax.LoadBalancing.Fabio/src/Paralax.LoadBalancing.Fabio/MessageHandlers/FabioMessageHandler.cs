using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Paralax.LoadBalancing.Fabio.MessageHandlers
{
    internal sealed class FabioMessageHandler : DelegatingHandler
    {
        private readonly FabioOptions _options;
        private readonly string _servicePath;

        public FabioMessageHandler(FabioOptions options, string serviceName = null)
        {
            if (string.IsNullOrWhiteSpace(options.Url))
            {
                throw new InvalidOperationException("Fabio URL was not provided.");
            }

            _options = options;
            _servicePath = string.IsNullOrWhiteSpace(serviceName) ? string.Empty : $"{serviceName}/";
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (!_options.Enabled)
            {
                // If Fabio is not enabled, use the default behavior.
                return base.SendAsync(request, cancellationToken);
            }

            // Modify the request URI to route through Fabio
            request.RequestUri = GetRequestUri(request);

            return base.SendAsync(request, cancellationToken);
        }

        private Uri GetRequestUri(HttpRequestMessage request)
        {
            // Construct the new request URI by prepending the Fabio URL and service path.
            var fabioUri = new Uri($"{_options.Url}/{_servicePath}{request.RequestUri.Host}{request.RequestUri.PathAndQuery}");
            return fabioUri;
        }
    }
}
