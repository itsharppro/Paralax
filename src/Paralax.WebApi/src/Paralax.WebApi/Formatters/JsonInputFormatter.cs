using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Open.Serialization.Json;

namespace Paralax.WebApi.Formatters
{
    internal class JsonInputFormatter : IInputFormatter
    {
        private const string EmptyJson = "{}";
        private readonly ConcurrentDictionary<Type, MethodInfo> _methods = new();
        private readonly IJsonSerializer _serializer;
        private readonly MethodInfo _deserializeMethod;

        public JsonInputFormatter(IJsonSerializer serializer)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));

            _deserializeMethod = _serializer.GetType().GetMethods()
                .Single(m => m.IsGenericMethod && m.Name == nameof(_serializer.Deserialize));
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

            try
            {
                var result = method.Invoke(_serializer, new object[] { json });

                return await InputFormatterResult.SuccessAsync(result);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Invalid JSON format", ex);
            }
        }
    }
}
