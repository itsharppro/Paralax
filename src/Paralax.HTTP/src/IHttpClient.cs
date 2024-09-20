using System.Net.Http;
using System.Threading.Tasks;

namespace Paralax.HTTP
{
    public interface IHttpClient : IHttpClientBase, 
        IHttpClientWithSerialization, 
        IHttpClientWithResult, 
        IHttpClientAdvanced
    {
    }
}
