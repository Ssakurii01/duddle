#if NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif

namespace Luxodd.Game.Scripts.Network
{
    public class UserDataPayload
    {
#if NEWTONSOFT_JSON
        [JsonProperty("user_data")]public object Data { get; set; }
#else
        public object Data { get; set; }
#endif
    }
}