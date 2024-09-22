using System.Collections.Generic;

namespace Paralax.HTTP
{
    public class HttpClientOptions
    {
        public string Type { get; set; }
        public int Retries { get; set; } = 3;
        public IDictionary<string, string> Services { get; set; } = new Dictionary<string, string>();
        public RequestMaskingOptions RequestMasking { get; set; } = new RequestMaskingOptions();
        public bool RemoveCharsetFromContentType { get; set; }
        public string CorrelationContextHeader { get; set; }
        public string CorrelationIdHeader { get; set; }
        public class RequestMaskingOptions
        {
            public bool Enabled { get; set; }
            public IEnumerable<string> UrlParts { get; set; } = new List<string>();
            public string MaskTemplate { get; set; } = "*****";
        }
    }
}
