using Newtonsoft.Json;

namespace CustomAlerts.Streamlabs
{
    internal struct HandshakeResponse
    {
        [JsonProperty("pingTimeout")]
        public int PingTimeout { get; set; }
    }
}