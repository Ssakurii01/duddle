#if NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif

namespace Luxodd.Game.Scripts.Network
{
    public class CommandResponse
    {
#if NEWTONSOFT_JSON
        [JsonProperty("msgver")] public string MessageVersion { get; set; }
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("ts")] public string TimeStamp { get; set; }
        [JsonProperty("status")] public int StatusCode { get; set; }
        [JsonProperty("payload")] public object Payload { get; set; }
#else
        public string MessageVersion { get; set; }
        public string Type { get; set; }
        public string TimeStamp { get; set; }
        public int StatusCode { get; set; }
        public object Payload { get; set; }
#endif
    }
}