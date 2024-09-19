using System.Collections.Generic;
using System.Net.Http.Headers;

namespace Paralax.HTTP
{
    public interface IHttpClientHeaders
    {
        void SetHeaders(IDictionary<string, string> headers);
        void SetHeaders(Action<HttpRequestHeaders> headers);
    }
}
