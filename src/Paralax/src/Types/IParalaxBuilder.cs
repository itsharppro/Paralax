using Microsoft.Extensions.DependencyInjection;

namespace Paralax.Types
{
    public interface IParalaxBuilder
    {
        IServiceCollection Services { get; }
        bool TryRegister(string name);
        void AddBuildAction(System.Action<IServiceProvider> execute);
        IServiceProvider Build();
        void AddInitializer(IInitializer initializer); 
        void AddInitializer<TInitializer>() where TInitializer : IInitializer; 
    }
}
