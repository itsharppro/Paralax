using System.Threading.Tasks;

namespace Paralax.Types
{
    public interface IInitializer
    {
        Task InitializeAsync();
    }
}
