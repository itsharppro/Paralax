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
                SkipDefaultValue = false,     
                DateFormat = NetJSONDateFormat.ISO,
                TimeZoneFormat = NetJSONTimeZoneFormat.Utc
            };
        }

        public string Serialize<T>(T value)
        {
            return NetJSON.NetJSON.Serialize(value, _settings);
        }

        public async Task SerializeAsync<T>(Stream stream, T value)
        {
            var json = NetJSON.NetJSON.Serialize(value, _settings);  

            using (var writer = new StreamWriter(stream)) 
            {
                await writer.WriteAsync(json);
                await writer.FlushAsync();
            }
        }

        public async ValueTask<T> DeserializeAsync<T>(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                var content = await reader.ReadToEndAsync();

                var result = NetJSON.NetJSON.Deserialize<T>(content, _settings);
                
                return result;
            }
        }

        public T Deserialize<T>(string json)
        {
            return NetJSON.NetJSON.Deserialize<T>(json, _settings);
        }
    }
}
