using System.Collections.Generic;

namespace Paralax.HTTP
{
    public class HttpClientOptions
    {
        /// <summary>
        /// The type of the HTTP client, used to differentiate between various client configurations.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The number of retry attempts to be used for transient errors.
        /// </summary>
        public int Retries { get; set; } = 3;

        /// <summary>
        /// A dictionary of services that may be utilized by the HTTP client.
        /// </summary>
        public IDictionary<string, string> Services { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Configuration options for masking parts of the request URL.
        /// </summary>
        public RequestMaskingOptions RequestMasking { get; set; } = new RequestMaskingOptions();

        /// <summary>
        /// If true, the charset parameter is removed from the Content-Type header.
        /// </summary>
        public bool RemoveCharsetFromContentType { get; set; }

        /// <summary>
        /// The HTTP header used to store the correlation context (if applicable).
        /// </summary>
        public string CorrelationContextHeader { get; set; }

        /// <summary>
        /// The HTTP header used to store the correlation ID (if applicable).
        /// </summary>
        public string CorrelationIdHeader { get; set; }

        /// <summary>
        /// Options related to request URL masking.
        /// </summary>
        public class RequestMaskingOptions
        {
            /// <summary>
            /// Indicates whether request URL masking is enabled.
            /// </summary>
            public bool Enabled { get; set; }

            /// <summary>
            /// A collection of URL parts to be masked in the logs.
            /// </summary>
            public IEnumerable<string> UrlParts { get; set; } = new List<string>();

            /// <summary>
            /// The template used to mask sensitive parts of the URL.
            /// </summary>
            public string MaskTemplate { get; set; } = "*****";
        }
    }
}
