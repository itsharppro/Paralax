using System.Text.Json.Serialization;

namespace Paralax.Discovery.Consul.Models
{
    public class Connect
    {
        [JsonPropertyName("sidecar_service")]
        public SidecarService SidecarService { get; set; }
    }
}