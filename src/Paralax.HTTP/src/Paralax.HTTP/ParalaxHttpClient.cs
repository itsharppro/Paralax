using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Polly;

namespace Paralax.HTTP
{
    public class ParalaxHttpClient : IHttpClient
    {
        private const string JsonContentType = "application/json";
        private readonly HttpClient _client;
        private readonly HttpClientOptions _options;
        private readonly IHttpClientSerializer _serializer;

        public ParalaxHttpClient(HttpClient client, HttpClientOptions options, IHttpClientSerializer serializer,
            ICorrelationContextFactory correlationContextFactory, ICorrelationIdFactory correlationIdFactory)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));

            // Set default headers for correlation context and correlation ID
            if (!string.IsNullOrWhiteSpace(_options.CorrelationContextHeader))
            {
                var correlationContext = correlationContextFactory.Create();
                _client.DefaultRequestHeaders.TryAddWithoutValidation(_options.CorrelationContextHeader, correlationContext);
            }

            if (!string.IsNullOrWhiteSpace(_options.CorrelationIdHeader))
            {
                var correlationId = correlationIdFactory.Create();
                _client.DefaultRequestHeaders.TryAddWithoutValidation(_options.CorrelationIdHeader, correlationId);
            }
        }

        // Overloaded methods for GET, POST, PUT, PATCH, DELETE with proper serializer and content handling.
        public virtual Task<HttpResponseMessage> GetAsync(string uri)
            => SendAsync(uri, Method.Get);

        public virtual Task<T> GetAsync<T>(string uri, IHttpClientSerializer serializer = null)
            => SendAsync<T>(uri, Method.Get, null, serializer);

        public Task<HttpResult<T>> GetResultAsync<T>(string uri, IHttpClientSerializer serializer = null)
            => SendResultAsync<T>(uri, Method.Get, null, serializer);

        public virtual Task<HttpResponseMessage> PostAsync(string uri, object data = null, IHttpClientSerializer serializer = null)
            => SendAsync(uri, Method.Post, GetJsonPayload(data, serializer));

        public Task<HttpResponseMessage> PostAsync(string uri, HttpContent content)
            => SendAsync(uri, Method.Post, content);

        public virtual Task<T> PostAsync<T>(string uri, object data = null, IHttpClientSerializer serializer = null)
            => SendAsync<T>(uri, Method.Post, GetJsonPayload(data, serializer), serializer);

        public Task<T> PostAsync<T>(string uri, HttpContent content, IHttpClientSerializer serializer = null)
            => SendAsync<T>(uri, Method.Post, content, serializer);

        public Task<HttpResult<T>> PostResultAsync<T>(string uri, object data = null, IHttpClientSerializer serializer = null)
            => SendResultAsync<T>(uri, Method.Post, GetJsonPayload(data, serializer), serializer);

        public Task<HttpResult<T>> PostResultAsync<T>(string uri, HttpContent content, IHttpClientSerializer serializer = null)
            => SendResultAsync<T>(uri, Method.Post, content, serializer);

        public virtual Task<HttpResponseMessage> PutAsync(string uri, object data = null, IHttpClientSerializer serializer = null)
            => SendAsync(uri, Method.Put, GetJsonPayload(data, serializer));

        public Task<HttpResponseMessage> PutAsync(string uri, HttpContent content)
            => SendAsync(uri, Method.Put, content);

        public virtual Task<T> PutAsync<T>(string uri, object data = null, IHttpClientSerializer serializer = null)
            => SendAsync<T>(uri, Method.Put, GetJsonPayload(data, serializer), serializer);

        public Task<T> PutAsync<T>(string uri, HttpContent content, IHttpClientSerializer serializer = null)
            => SendAsync<T>(uri, Method.Put, content, serializer);

        public Task<HttpResult<T>> PutResultAsync<T>(string uri, object data = null, IHttpClientSerializer serializer = null)
            => SendResultAsync<T>(uri, Method.Put, GetJsonPayload(data, serializer), serializer);

        public Task<HttpResult<T>> PutResultAsync<T>(string uri, HttpContent content, IHttpClientSerializer serializer = null)
            => SendResultAsync<T>(uri, Method.Put, content, serializer);

        public Task<HttpResponseMessage> PatchAsync(string uri, object data = null, IHttpClientSerializer serializer = null)
            => SendAsync(uri, Method.Patch, GetJsonPayload(data, serializer));

        public Task<HttpResponseMessage> PatchAsync(string uri, HttpContent content)
            => SendAsync(uri, Method.Patch, content);

        public Task<T> PatchAsync<T>(string uri, object data = null, IHttpClientSerializer serializer = null)
            => SendAsync<T>(uri, Method.Patch, GetJsonPayload(data, serializer), serializer);

        public Task<T> PatchAsync<T>(string uri, HttpContent content, IHttpClientSerializer serializer = null)
            => SendAsync<T>(uri, Method.Patch, content, serializer);

        public Task<HttpResult<T>> PatchResultAsync<T>(string uri, object data = null, IHttpClientSerializer serializer = null)
            => SendResultAsync<T>(uri, Method.Patch, GetJsonPayload(data, serializer), serializer);

        public Task<HttpResult<T>> PatchResultAsync<T>(string uri, HttpContent content, IHttpClientSerializer serializer = null)
            => SendResultAsync<T>(uri, Method.Patch, content, serializer);

        public virtual Task<HttpResponseMessage> DeleteAsync(string uri)
            => SendAsync(uri, Method.Delete);

        public Task<T> DeleteAsync<T>(string uri, IHttpClientSerializer serializer = null)
            => SendAsync<T>(uri, Method.Delete, null, serializer);

        public Task<HttpResult<T>> DeleteResultAsync<T>(string uri, IHttpClientSerializer serializer = null)
            => SendResultAsync<T>(uri, Method.Delete, null, serializer);

        // Advanced SendAsync methods
        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
            => RetryPolicy(() => _client.SendAsync(request));

        public Task<T> SendAsync<T>(HttpRequestMessage request, IHttpClientSerializer serializer = null)
            => RetryPolicy(async () =>
            {
                var response = await _client.SendAsync(request);
                return await DeserializeResponse<T>(response, serializer);
            });

        public Task<HttpResult<T>>SendResultAsync<T>(HttpRequestMessage request, IHttpClientSerializer serializer = null)
            => RetryPolicy(async () =>
            {
                var response = await _client.SendAsync(request);
                return await CreateHttpResult<T>(response, serializer);
            });

        // Headers Management
        public void SetHeaders(IDictionary<string, string> headers)
        {
            if (headers == null) return;

            foreach (var header in headers)
            {
                if (!string.IsNullOrEmpty(header.Key))
                {
                    _client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }

        public void SetHeaders(Action<HttpRequestHeaders> headers)
        {
            headers?.Invoke(_client.DefaultRequestHeaders);
        }

        // Utility methods for sending HTTP requests
        private async Task<HttpResponseMessage> SendAsync(string uri, Method method, HttpContent content = null)
        {
            var request = new HttpRequestMessage(method.ToHttpMethod(), uri) { Content = content };
            return await RetryPolicy(() => _client.SendAsync(request));
        }

        private async Task<T> SendAsync<T>(string uri, Method method, HttpContent content = null, IHttpClientSerializer serializer = null)
        {
            var response = await SendAsync(uri, method, content);
            return await DeserializeResponse<T>(response, serializer);
        }

        private async Task<T> DeserializeResponse<T>(HttpResponseMessage response, IHttpClientSerializer serializer = null)
        {
            if (!response.IsSuccessStatusCode) return default;

            var stream = await response.Content.ReadAsStreamAsync();
            serializer ??= _serializer;

            return await serializer.DeserializeAsync<T>(stream);
        }

        private async Task<HttpResult<T>> CreateHttpResult<T>(HttpResponseMessage response, IHttpClientSerializer serializer = null)
        {
            if (!response.IsSuccessStatusCode)
            {
                return new HttpResult<T>(default, response);
            }

            var stream = await response.Content.ReadAsStreamAsync();
            serializer ??= _serializer;
            var result = await serializer.DeserializeAsync<T>(stream);
             Console.WriteLine($"Deserialized Result: {result}");

            return new HttpResult<T>(result, response);
        }

        private StringContent GetJsonPayload(object data, IHttpClientSerializer serializer = null)
        {
            if (data == null) return null;

            serializer ??= _serializer;
            var content = new StringContent(serializer.Serialize(data), Encoding.UTF8, JsonContentType);
            if (_options.RemoveCharsetFromContentType && content.Headers.ContentType != null)
            {
                content.Headers.ContentType.CharSet = null;
            }

            return content;
        }

        private async Task<T> RetryPolicy<T>(Func<Task<T>> action)
        {
            return await Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(_options.Retries, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
                .ExecuteAsync(action);
        }

        protected virtual async Task<HttpResult<T>> SendResultAsync<T>(string uri, Method method, HttpContent content = null, IHttpClientSerializer serializer = null)
        {
            var response = await SendAsync(uri, method, content);
            
            if (!response.IsSuccessStatusCode)
            {
                return new HttpResult<T>(default, response);
            }

            var stream = await response.Content.ReadAsStreamAsync();
            
            var result = await DeserializeJsonFromStream<T>(stream, serializer);
            Console.WriteLine($"Deserialized Result: {result}");
            
            return new HttpResult<T>(result, response);
        }

        private async Task<T> DeserializeJsonFromStream<T>(Stream stream, IHttpClientSerializer serializer = null)
        {
            if (stream == null || !stream.CanRead)
            {
                return default;
            }

            serializer ??= _serializer;

            return await serializer.DeserializeAsync<T>(stream);
        }



        public enum Method
        {
            Get,
            Post,
            Put,
            Patch,
            Delete
        }
    }

    internal static class MethodExtensions
    {
        public static HttpMethod ToHttpMethod(this ParalaxHttpClient.Method method)
        {
            return method switch
            {
                ParalaxHttpClient.Method.Get => HttpMethod.Get,
                ParalaxHttpClient.Method.Post => HttpMethod.Post,
                ParalaxHttpClient.Method.Put => HttpMethod.Put,
                ParalaxHttpClient.Method.Patch => HttpMethod.Patch,
                ParalaxHttpClient.Method.Delete => HttpMethod.Delete,
                _ => throw new InvalidOperationException($"Unsupported HTTP method: {method}")
            };
        }
    }
}
