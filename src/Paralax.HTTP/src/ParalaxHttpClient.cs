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
    public class ParalaxHttpClient : IHttpClientBase, IHttpClientWithSerialization, IHttpClientWithResult, IHttpClientAdvanced, IHttpClientHeaders
    {
        private const string JsonContentType = "application/json";
        private readonly HttpClient _client;
        private readonly IHttpClientSerializer _serializer;
        private readonly int _retries;

        public ParalaxHttpClient(HttpClient client, IHttpClientSerializer serializer, int retries = 3)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _retries = retries;
        }

        // IHttpClientBase implementation

        public Task<HttpResponseMessage> GetAsync(string uri) => SendAsync(uri, HttpMethod.Get);
        public Task<HttpResponseMessage> PostAsync(string uri, HttpContent content) => SendAsync(uri, HttpMethod.Post, content);
        public Task<HttpResponseMessage> PutAsync(string uri, HttpContent content) => SendAsync(uri, HttpMethod.Put, content);
        public Task<HttpResponseMessage> PatchAsync(string uri, HttpContent content) => SendAsync(uri, HttpMethod.Patch, content);
        public Task<HttpResponseMessage> DeleteAsync(string uri) => SendAsync(uri, HttpMethod.Delete);

        // IHttpClientWithSerialization implementation

        public async Task<T> GetAsync<T>(string uri, IHttpClientSerializer serializer = null)
        {
            var response = await SendAsync(uri, HttpMethod.Get).ConfigureAwait(false);
            return await DeserializeResponse<T>(response, serializer).ConfigureAwait(false);
        }

        public async Task<T> PostAsync<T>(string uri, object data = null, IHttpClientSerializer serializer = null)
        {
            var content = GetJsonPayload(data, serializer);
            var response = await SendAsync(uri, HttpMethod.Post, content).ConfigureAwait(false);
            return await DeserializeResponse<T>(response, serializer).ConfigureAwait(false);
        }

        public async Task<T> PutAsync<T>(string uri, object data = null, IHttpClientSerializer serializer = null)
        {
            var content = GetJsonPayload(data, serializer);
            var response = await SendAsync(uri, HttpMethod.Put, content).ConfigureAwait(false);
            return await DeserializeResponse<T>(response, serializer).ConfigureAwait(false);
        }

        public async Task<T> PatchAsync<T>(string uri, object data = null, IHttpClientSerializer serializer = null)
        {
            var content = GetJsonPayload(data, serializer);
            var response = await SendAsync(uri, HttpMethod.Patch, content).ConfigureAwait(false);
            return await DeserializeResponse<T>(response, serializer).ConfigureAwait(false);
        }

        public async Task<T> DeleteAsync<T>(string uri, IHttpClientSerializer serializer = null)
        {
            var response = await SendAsync(uri, HttpMethod.Delete).ConfigureAwait(false);
            return await DeserializeResponse<T>(response, serializer).ConfigureAwait(false);
        }

        // IHttpClientWithResult implementation

        public async Task<HttpResult<T>> GetResultAsync<T>(string uri, IHttpClientSerializer serializer = null)
        {
            var response = await SendAsync(uri, HttpMethod.Get).ConfigureAwait(false);
            return await CreateHttpResult<T>(response, serializer).ConfigureAwait(false);
        }

        public async Task<HttpResult<T>> PostResultAsync<T>(string uri, object data = null, IHttpClientSerializer serializer = null)
        {
            var content = GetJsonPayload(data, serializer);
            var response = await SendAsync(uri, HttpMethod.Post, content).ConfigureAwait(false);
            return await CreateHttpResult<T>(response, serializer).ConfigureAwait(false);
        }

        public async Task<HttpResult<T>> PutResultAsync<T>(string uri, object data = null, IHttpClientSerializer serializer = null)
        {
            var content = GetJsonPayload(data, serializer);
            var response = await SendAsync(uri, HttpMethod.Put, content).ConfigureAwait(false);
            return await CreateHttpResult<T>(response, serializer).ConfigureAwait(false);
        }

        public async Task<HttpResult<T>> PatchResultAsync<T>(string uri, object data = null, IHttpClientSerializer serializer = null)
        {
            var content = GetJsonPayload(data, serializer);
            var response = await SendAsync(uri, HttpMethod.Patch, content).ConfigureAwait(false);
            return await CreateHttpResult<T>(response, serializer).ConfigureAwait(false);
        }

        public async Task<HttpResult<T>> DeleteResultAsync<T>(string uri, IHttpClientSerializer serializer = null)
        {
            var response = await SendAsync(uri, HttpMethod.Delete).ConfigureAwait(false);
            return await CreateHttpResult<T>(response, serializer).ConfigureAwait(false);
        }

        // IHttpClientAdvanced implementation

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
            => RetryPolicy<HttpResponseMessage>(() => _client.SendAsync(request));

        public async Task<T> SendAsync<T>(HttpRequestMessage request, IHttpClientSerializer serializer = null)
        {
            var response = await SendAsync(request).ConfigureAwait(false);
            return await DeserializeResponse<T>(response, serializer).ConfigureAwait(false);
        }

        public async Task<HttpResult<T>> SendResultAsync<T>(HttpRequestMessage request, IHttpClientSerializer serializer = null)
        {
            var response = await SendAsync(request).ConfigureAwait(false);
            return await CreateHttpResult<T>(response, serializer).ConfigureAwait(false);
        }

        // IHttpClientHeaders implementation

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

        // Private helper methods

        private async Task<HttpResponseMessage> SendAsync(string uri, HttpMethod method, HttpContent content = null)
        {
            var request = new HttpRequestMessage(method, uri) { Content = content };
            return await RetryPolicy<HttpResponseMessage>(() => _client.SendAsync(request));
        }

        private async Task<T> DeserializeResponse<T>(HttpResponseMessage response, IHttpClientSerializer serializer = null)
        {
            if (!response.IsSuccessStatusCode) return default;

            var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            serializer ??= _serializer;

            return await serializer.DeserializeAsync<T>(stream).ConfigureAwait(false);
        }

        private async Task<HttpResult<T>> CreateHttpResult<T>(HttpResponseMessage response, IHttpClientSerializer serializer = null)
        {
            if (!response.IsSuccessStatusCode)
            {
                return new HttpResult<T>(default, response);
            }

            var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            serializer ??= _serializer;
            var result = await serializer.DeserializeAsync<T>(stream).ConfigureAwait(false);

            return new HttpResult<T>(result, response);
        }

        private StringContent GetJsonPayload(object data, IHttpClientSerializer serializer = null)
        {
            if (data == null) return null;

            serializer ??= _serializer;
            var content = new StringContent(serializer.Serialize(data), Encoding.UTF8, JsonContentType);
            return content;
        }

        private async Task<T> RetryPolicy<T>(Func<Task<T>> action)
        {
            return await Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(_retries, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
                .ExecuteAsync(action).ConfigureAwait(false);
        }
    }
}
