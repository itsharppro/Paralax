using System;
using System.Diagnostics;

namespace Paralax.gRPC.Utils
{
    public sealed class UptimeService
    {
        public long GetUptimeSeconds()
        {
            var uptime = DateTimeOffset.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
            return (long)uptime.TotalSeconds;
        }
    }
}
