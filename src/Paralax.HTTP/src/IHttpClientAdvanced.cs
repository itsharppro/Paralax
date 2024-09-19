using System.Net.Http;
using System.Threading.Tasks;

namespace Paralax.HTTP
{
    public interface IHttpClientAdvanced : IHttpClientBase
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
        Task<T> SendAsync<T>(HttpRequestMessage request, IHttpClientSerializer serializer = null);
        Task<HttpResult<T>> SendResultAsync<T>(HttpRequestMessage request, IHttpClientSerializer serializer = null);
    }
}
