using System.Threading.Tasks;

namespace Paralax.HTTP
{
    public interface IHttpClientWithResult : IHttpClientBase
    {
        Task<HttpResult<T>> GetResultAsync<T>(string uri, IHttpClientSerializer serializer = null);
        Task<HttpResult<T>> PostResultAsync<T>(string uri, object data = null, IHttpClientSerializer serializer = null);
        Task<HttpResult<T>> PutResultAsync<T>(string uri, object data = null, IHttpClientSerializer serializer = null);
        Task<HttpResult<T>> PatchResultAsync<T>(string uri, object data = null, IHttpClientSerializer serializer = null);
        Task<HttpResult<T>> DeleteResultAsync<T>(string uri, IHttpClientSerializer serializer = null);
    }
}
