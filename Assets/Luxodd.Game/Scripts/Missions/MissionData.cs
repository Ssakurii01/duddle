using UnityEngine;
using UnityEngine.Serialization;

namespace Luxodd.Game.Scripts.Missions
{
    [CreateAssetMenu(menuName = "Unity Plugin/Missions/Mission Data", fileName = "MissionData", order = 1)]
    public class MissionData : ScriptableObject
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField, TextArea] public string Description { get; private set; }
        [field: SerializeField] public MissionType Type { get; private set; }
        [field: SerializeField] public DifficultyLevel DifficultyLevel { get; private set; }
        [field: SerializeField, Range(0, 100)] public int Hardness { get; private set; }
        [field: SerializeField] public float Bet { get; private set; }
        [field: SerializeField] public float Ratio { get; private set; }
        [field: SerializeField] public int Value { get; private set; }
        [field: SerializeField] public int Level { get; private set; }
    }
}
