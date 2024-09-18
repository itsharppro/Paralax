using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Paralax.Types;

namespace Paralax.Core
{
    public class StartupInitializer : IStartupInitializer
    {
        private readonly IList<IInitializer> _initializers = new List<IInitializer>();

        public IReadOnlyCollection<IInitializer> Initializers => new ReadOnlyCollection<IInitializer>(_initializers);

        public void AddInitializer(IInitializer initializer)
        {
            if (initializer is null || _initializers.Contains(initializer))
            {
                return;
            }

            _initializers.Add(initializer);
        }

        public async Task InitializeAsync()
        {
            foreach (var initializer in _initializers)
            {
                await initializer.InitializeAsync();
            }
        }
    }
}
