using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Paralax.MessageBrokers.RabbitMQ
{
    public interface IRabbitMqSerializer
    {
        ReadOnlySpan<byte> Serialize(object value);
        object Deserialize(ReadOnlySpan<byte> value, Type type);
        object Deserialize(ReadOnlySpan<byte> value);
    }
}