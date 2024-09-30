using System.IO;
using System.Threading.Tasks;
using NetJSON;

namespace Paralax.HTTP
{
    public class NetJsonHttpClientSerializer : IHttpClientSerializer
    {
        private readonly NetJSONSettings _settings;

        public NetJsonHttpClientSerializer(NetJSONSettings settings = null)
        {
            _settings = settings ?? new NetJSONSettings
            {
                UseEnumString = true,   
                CaseSensitive = false, 
                SkipDefaultValue = true 
            };
        }

        public string Serialize<T>(T value) => NetJSON.NetJSON.Serialize(value, _settings);

        public async Task SerializeAsync<T>(Stream stream, T value)
        {
            using (var writer = new StreamWriter(stream))
            {
                await writer.WriteAsync(NetJSON.NetJSON.Serialize(value, _settings));
                await writer.FlushAsync();
            }
        }

        public async ValueTask<T> DeserializeAsync<T>(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                var content = await reader.ReadToEndAsync();
                return NetJSON.NetJSON.Deserialize<T>(content, _settings);
            }
        }

        public T Deserialize<T>(string json) => NetJSON.NetJSON.Deserialize<T>(json, _settings);
    }
}
