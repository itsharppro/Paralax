using System;
using Paralax.Types;

namespace Paralax
{
    internal class ServiceId : IServiceId
    {
        public string Id { get; } = $"{Guid.NewGuid():N}";
    }
}
