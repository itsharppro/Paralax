using System.IO;
using System.Threading.Tasks;
using MessagePack;
using MessagePack.Resolvers;

namespace Paralax.HTTP
{
    public class MessagePackHttpClientSerializer : IHttpClientSerializer
    {
        private readonly MessagePackSerializerOptions _options;

        public MessagePackHttpClientSerializer(MessagePackSerializerOptions options = null)
        {
            _options = options ?? MessagePackSerializerOptions.Standard
                .WithResolver(ContractlessStandardResolver.Instance)  
                .WithCompression(MessagePackCompression.Lz4BlockArray) 
                .WithSecurity(MessagePackSecurity.UntrustedData);  
        }

        public string Serialize<T>(T value)
        {
            var bytes = MessagePackSerializer.Serialize(value, _options);
            return System.Convert.ToBase64String(bytes);
        }

        public async Task SerializeAsync<T>(Stream stream, T value)
        {
            await MessagePackSerializer.SerializeAsync(stream, value, _options);
        }

        public async ValueTask<T> DeserializeAsync<T>(Stream stream)
        {
            return await MessagePackSerializer.DeserializeAsync<T>(stream, _options);
        }

        public T Deserialize<T>(string base64)
        {
            var bytes = System.Convert.FromBase64String(base64);
            return MessagePackSerializer.Deserialize<T>(bytes, _options);
        }
    }
}
