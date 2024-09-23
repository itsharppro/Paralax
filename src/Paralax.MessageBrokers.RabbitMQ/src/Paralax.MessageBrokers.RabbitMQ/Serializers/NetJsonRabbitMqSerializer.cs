using System;
using NetJSON;

namespace Paralax.MessageBrokers.RabbitMQ.Serializers
{
    public sealed class NetJsonRabbitMqSerializer : IRabbitMqSerializer
    {
        public NetJsonRabbitMqSerializer()
        {
            NetJSON.NetJSON.IncludeTypeInformation = false;
            NetJSON.NetJSON.CaseSensitive = false;
            NetJSON.NetJSON.UseEnumString = true;
            

            // Note: No CamelCase property in NetJSON???
        }

        public ReadOnlySpan<byte> Serialize(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var jsonString = NetJSON.NetJSON.Serialize(value);

            return System.Text.Encoding.UTF8.GetBytes(jsonString);
        }

        public object Deserialize(ReadOnlySpan<byte> value, Type type)
        {
            if (value.IsEmpty)
                throw new ArgumentException("Value cannot be empty.", nameof(value));

            var jsonString = System.Text.Encoding.UTF8.GetString(value);

            var method = typeof(NetJSON.NetJSON).GetMethod("Deserialize", new[] { typeof(string) });
            var genericMethod = method.MakeGenericMethod(type);
            
            return genericMethod.Invoke(null, new object[] { jsonString });
        }

        public object Deserialize(ReadOnlySpan<byte> value)
        {
            if (value.IsEmpty)
                throw new ArgumentException("Value cannot be empty.", nameof(value));

            var jsonString = System.Text.Encoding.UTF8.GetString(value);

            return NetJSON.NetJSON.Deserialize<object>(jsonString);
        }
    }
}
