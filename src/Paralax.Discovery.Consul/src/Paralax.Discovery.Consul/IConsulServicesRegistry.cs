using System.Threading.Tasks;
using Paralax.Discovery.Consul.Models;

namespace Paralax.Discovery.Consul
{
    public interface IConsulServicesRegistry
    {
        Task<ServiceAgent> GetAsync(string name);
    }
}
