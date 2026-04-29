#if NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif

namespace Luxodd.Game.Scripts.Network.Payloads
{
    public class AmountPayload
    {
#if NEWTONSOFT_JSON
        [JsonProperty("amount")] public int Amount { get; set; }
        [JsonProperty("pin")] public string PinCode { get; set; }
#else
        public int Amount { get; set; }
        public string PinCode { get; set; }
#endif
    }
}