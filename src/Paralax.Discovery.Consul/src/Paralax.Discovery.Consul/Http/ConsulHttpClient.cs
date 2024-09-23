using System.Net.Http;
using Paralax.HTTP;

namespace Paralax.Discovery.Consul.Http
{
    internal sealed class ConsulHttpClient : ParalaxHttpClient, IConsulHttpClient
    {
        public ConsulHttpClient(HttpClient client, HttpClientOptions options, IHttpClientSerializer serializer,
            ICorrelationContextFactory correlationContextFactory, ICorrelationIdFactory correlationIdFactory)
            : base(client, options, serializer, correlationContextFactory, correlationIdFactory)
        {
        }
    }
}
