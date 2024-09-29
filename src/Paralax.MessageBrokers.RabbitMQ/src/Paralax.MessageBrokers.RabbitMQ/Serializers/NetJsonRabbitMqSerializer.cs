using System;
using NetJSON;
using System.Text;

namespace Paralax.MessageBrokers.RabbitMQ.Serializers
{
    public sealed class NetJsonRabbitMqSerializer : IRabbitMqSerializer
    {
        public NetJsonRabbitMqSerializer()
        {
            NetJSON.NetJSON.IncludeTypeInformation = false;
            NetJSON.NetJSON.CaseSensitive = false;
            NetJSON.NetJSON.UseEnumString = true;
        }

        public ReadOnlySpan<byte> Serialize(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var jsonString = NetJSON.NetJSON.Serialize(value);

            return Encoding.UTF8.GetBytes(jsonString);
        }

        public object Deserialize(ReadOnlySpan<byte> value, Type type)
        {
            if (value.IsEmpty)
                throw new ArgumentException("Value cannot be empty.", nameof(value));

            var jsonString = Encoding.UTF8.GetString(value);

            return NetJSON.NetJSON.Deserialize(type, jsonString);
        }

        public object Deserialize(ReadOnlySpan<byte> value)
        {
            if (value.IsEmpty)
                throw new ArgumentException("Value cannot be empty.", nameof(value));

            var jsonString = Encoding.UTF8.GetString(value);

            return NetJSON.NetJSON.Deserialize<object>(jsonString);
        }
    }
}
