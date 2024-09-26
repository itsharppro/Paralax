using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Paralax.Secrets.Vault.Core
{
    internal sealed class LeaseService : ILeaseService
    {
        private static readonly ConcurrentDictionary<string, LeaseData> Secrets =
            new ConcurrentDictionary<string, LeaseData>();

        public IReadOnlyDictionary<string, LeaseData> All => Secrets;

        public LeaseData Get(string key)
        {
            LeaseData data;
            Secrets.TryGetValue(key, out data);
            return data;
        }

        public void Set(string key, LeaseData data)
        {
            Secrets.TryRemove(key, out _);
            Secrets.TryAdd(key, data);
        }
    }
}