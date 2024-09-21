using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using NetJSON;

namespace Paralax.WebApi.Formatters
{
    internal class JsonInputFormatter : IInputFormatter
    {
        private const string EmptyJson = "{}";  
        private readonly ConcurrentDictionary<Type, MethodInfo> _methods = new();  
        private readonly MethodInfo _deserializeMethod;

        public JsonInputFormatter()
        {
            _deserializeMethod = typeof(NetJSON.NetJSON).GetMethods()
                .Single(m => m.IsGenericMethod && m.Name == nameof(NetJSON.NetJSON.Deserialize)); 
        }

        public bool CanRead(InputFormatterContext context)
        {
            return true;
        }

        public async Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
        {
            if (!_methods.TryGetValue(context.ModelType, out var method))
            {
                method = _deserializeMethod.MakeGenericMethod(context.ModelType);
                _methods.TryAdd(context.ModelType, method);
            }

            var request = context.HttpContext.Request;
            string json;

            using (var streamReader = new StreamReader(request.Body))
            {
                json = await streamReader.ReadToEndAsync();
            }

            if (string.IsNullOrWhiteSpace(json))
            {
                json = EmptyJson;
            }

            // Use NetJSON to deserialize the JSON string
            var result = method.Invoke(null, new object[] { json });

            return await InputFormatterResult.SuccessAsync(result);
        }
    }
}
