using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Paralax.Core;
using Paralax.Types;

namespace Paralax
{
    public sealed class ParalaxBuilder : IParalaxBuilder
    {
        private readonly ConcurrentDictionary<string, bool> _registry = new();
        private readonly List<Action<IServiceProvider>> _buildActions;
        private readonly IServiceCollection _services;
        public IServiceCollection Services => _services;

        public IConfiguration Configuration { get; }

        private ParalaxBuilder(IServiceCollection services, IConfiguration configuration)
        {
            _buildActions = new List<Action<IServiceProvider>>();
            _services = services;
            _services.AddSingleton<IStartupInitializer>(new StartupInitializer());
            Configuration = configuration;
        }

        public static IParalaxBuilder Create(IServiceCollection services, IConfiguration configuration = null)
            => new ParalaxBuilder(services, configuration);

        public bool TryRegister(string name) => _registry.TryAdd(name, true);

        public void AddBuildAction(Action<IServiceProvider> execute)
            => _buildActions.Add(execute);

        public void AddInitializer(IInitializer initializer)
            => AddBuildAction(sp =>
            {
                var startupInitializer = sp.GetRequiredService<IStartupInitializer>();
                startupInitializer.AddInitializer(initializer);
            });

        public void AddInitializer<TInitializer>() where TInitializer : IInitializer
            => AddBuildAction(sp =>
            {
                var initializer = sp.GetRequiredService<TInitializer>();
                var startupInitializer = sp.GetRequiredService<IStartupInitializer>();
                startupInitializer.AddInitializer(initializer);
            });

        public IServiceProvider Build()
        {
            var serviceProvider = _services.BuildServiceProvider();
            _buildActions.ForEach(a => a(serviceProvider));

            // Ensure that IStartupInitializer.InitializeAsync() is called after building the service provider
            var startupInitializer = serviceProvider.GetRequiredService<IStartupInitializer>();
            startupInitializer.InitializeAsync().GetAwaiter().GetResult(); 

            return serviceProvider;
        }

    }
}
