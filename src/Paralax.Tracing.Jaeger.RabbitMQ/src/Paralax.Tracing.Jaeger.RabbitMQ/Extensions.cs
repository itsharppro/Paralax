using Paralax.MessageBrokers.RabbitMQ;
using Paralax.Tracing.Jaeger.RabbitMQ.Plugins;

namespace Paralax.Tracing.Jaeger.RabbitMQ.
{

public static class Extensions
{
    public static IRabbitMqPluginsRegistry AddJaegerRabbitMqPlugin(this IRabbitMqPluginsRegistry registry)
    {
        registry.Add<JaegerPlugin>();
        return registry;
    }
}
}