using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Paralax.Discovery.Consul.Models;

namespace Paralax.Discovery.Consul.Services
{
    internal sealed class ConsulServicesRegistry : IConsulServicesRegistry
    {
        private readonly Random _random = new();
        private readonly IConsulService _consulService;
        private readonly IDictionary<string, ISet<string>> _usedServices = new Dictionary<string, ISet<string>>();

        public ConsulServicesRegistry(IConsulService consulService)
        {
            _consulService = consulService;
        }

        public async Task<ServiceAgent> GetAsync(string name)
        {
            var services = await _consulService.GetServiceAgentsAsync(name);
            if (!services.Any())
            {
                return null;
            }

            if (!_usedServices.ContainsKey(name))
            {
                _usedServices[name] = new HashSet<string>();
            }
            else if (services.Count == _usedServices[name].Count)
            {
                _usedServices[name].Clear();
            }

            return GetService(services, name);
        }

        private ServiceAgent GetService(IDictionary<string, ServiceAgent> services, string name)
        {
            return services.Count switch
            {
                0 => null,
                1 => services.First().Value,
                _ => ChooseService(services, name)
            };
        }

        private ServiceAgent ChooseService(IDictionary<string, ServiceAgent> services, string name)
        {
            ServiceAgent service;
            var unusedServices = services.Where(s => !_usedServices[name].Contains(s.Key)).ToArray();
            if (unusedServices.Any())
            {
                service = unusedServices.ElementAt(_random.Next(0, unusedServices.Count())).Value;
            }
            else
            {
                service = services.First().Value;
                _usedServices[name].Clear();
            }

            _usedServices[name].Add(service.Id);
            return service;
        }
    }
}
