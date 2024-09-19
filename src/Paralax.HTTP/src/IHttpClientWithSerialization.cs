using System.Threading.Tasks;

namespace Paralax.HTTP
{
    public interface IHttpClientWithSerialization : IHttpClientBase
    {
        Task<T> GetAsync<T>(string uri, IHttpClientSerializer serializer = null);
        Task<T> PostAsync<T>(string uri, object data = null, IHttpClientSerializer serializer = null);
        Task<T> PutAsync<T>(string uri, object data = null, IHttpClientSerializer serializer = null);
        Task<T> PatchAsync<T>(string uri, object data = null, IHttpClientSerializer serializer = null);
        Task<T> DeleteAsync<T>(string uri, IHttpClientSerializer serializer = null);
    }
}
