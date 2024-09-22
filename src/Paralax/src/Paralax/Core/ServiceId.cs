using System;
using Paralax.Types;

namespace Paralax.Core
{
    internal class ServiceId : IServiceId
    {
        public string Id { get; } = $"{Guid.NewGuid():N}";
    }
}
