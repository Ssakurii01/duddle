using System.Collections.Generic;

#if NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif
using UnityEngine;

namespace Luxodd.Game.Scripts.Missions
{
    public class MissionsExport 
    {
#if NEWTONSOFT_JSON
        [JsonProperty("missions")] public List<MissionDto> Missions { get; set; }
#else
        public List<MissionDto> Missions { get; set; }
#endif
    }
}
