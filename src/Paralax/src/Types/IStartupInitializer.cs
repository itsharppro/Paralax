using System.Threading.Tasks;

namespace Paralax.Types
{
    public interface IStartupInitializer
    {
        void AddInitializer(IInitializer initializer);
        Task InitializeAsync();
    }
}
