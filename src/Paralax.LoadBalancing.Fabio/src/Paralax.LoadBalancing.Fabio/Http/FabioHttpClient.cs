using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Paralax.HTTP;

namespace Paralax.LoadBalancing.Fabio.Http
{
    internal sealed class FabioHttpClient : ParalaxHttpClient, IFabioHttpClient
    {
        public FabioHttpClient(HttpClient client, HttpClientOptions options, IHttpClientSerializer serializer,
        ICorrelationContextFactory correlationContextFactory, ICorrelationIdFactory correlationIdFactory)
        : base(client, options, serializer, correlationContextFactory, correlationIdFactory)
        {
        }
    }
}