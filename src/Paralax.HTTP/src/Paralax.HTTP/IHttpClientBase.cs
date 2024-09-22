using System.Net.Http;
using System.Threading.Tasks;

namespace Paralax.HTTP
{
    public interface IHttpClientBase
    {
        Task<HttpResponseMessage> GetAsync(string uri);
        Task<HttpResponseMessage> PostAsync(string uri, HttpContent content);
        Task<HttpResponseMessage> PutAsync(string uri, HttpContent content);
        Task<HttpResponseMessage> PatchAsync(string uri, HttpContent content);
        Task<HttpResponseMessage> DeleteAsync(string uri);
    }
}
