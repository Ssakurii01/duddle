using System.Collections.Generic;
using UnityEngine;

namespace Luxodd.Game.Scripts.Missions
{
    [CreateAssetMenu(menuName = "Unity Plugin/Missions/Mission Data Base", fileName = "MissionDataBase", order = 2)]
    public class MissionDataBase : ScriptableObject
    {
        [field: SerializeField] public List<MissionData> Missions { get; private set; }

        public MissionData ProvideMissionDataById(string missionId)
        {
            var result = Missions.Find(missionData => missionData.Id == missionId);
            return result;
        }
        
        public MissionData ProvideMissionDataByName(string missionName)
        {
            var result = Missions.Find(missionData => missionData.Name == missionName);
            return result;
        }
    }
}
