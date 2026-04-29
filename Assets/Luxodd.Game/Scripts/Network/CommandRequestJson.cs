#if NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif

namespace Luxodd.Game.Scripts.Network
{
    public class CommandRequestJson
    {
#if NEWTONSOFT_JSON
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)] public string Version { get; set; }
        [JsonProperty("payload")] public object Payload { get; set; }
#else
        public string Type { get; set; }
        public string Version { get; set; }
        public object Payload { get; set; }
#endif
    }
}