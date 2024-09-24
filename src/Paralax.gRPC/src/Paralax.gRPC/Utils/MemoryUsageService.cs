using System;

namespace Paralax.gRPC.Utils
{
    public sealed class MemoryUsageService
    {
        public double GetMemoryUsage()
        {
            var totalMemory = GC.GetTotalMemory(forceFullCollection: false);
            return Math.Round((double)totalMemory / (1024 * 1024), 4); 
        }
    }
}
