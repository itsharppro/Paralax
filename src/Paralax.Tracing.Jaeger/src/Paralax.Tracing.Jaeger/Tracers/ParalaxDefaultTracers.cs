using Microsoft.Extensions.DependencyInjection;
using Jaeger;
using Jaeger.Reporters;
using Jaeger.Samplers;
using OpenTracing;

using System;
using System.Reflection;

namespace Paralax.Tracing.Jaeger.Tracers
{
    internal sealed class ParalaxDefaultTracer
{
    public static ITracer Create()
        => new Tracer.Builder(Assembly.GetEntryAssembly().FullName)
            .WithReporter(new NoopReporter())
            .WithSampler(new ConstSampler(false))
            .Build();
    }
}
