using System;
using System.Diagnostics;
using System.Threading;

namespace Paralax.gRPC.Utils
{
    public sealed class CpuUsageService
    {
        private readonly TimeSpan _interval = TimeSpan.FromMilliseconds(500); // Interval to sample CPU usage
        private readonly Process _process;

        public CpuUsageService()
        {
            _process = Process.GetCurrentProcess();
        }

        public double GetCpuUsage()
        {
            var startTime = _process.TotalProcessorTime;
            var startCpuUsage = DateTime.UtcNow;

            Thread.Sleep(_interval);

            var endTime = _process.TotalProcessorTime;
            var endCpuUsage = DateTime.UtcNow;

            var cpuUsedMs = (endTime - startTime).TotalMilliseconds;
            var totalTimePassedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;

            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalTimePassedMs) * 100;

            return Math.Round(cpuUsageTotal, 2);
        }
    }
}
