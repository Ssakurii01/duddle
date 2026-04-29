using UnityEngine;

namespace Luxodd.Game.Scripts.Input
{
    [CreateAssetMenu(menuName = "Unity Plugin/Arcade/Input Config Asset", fileName = "ArcadeInputConfigAsset", order = 0)]
    public class ArcadeInputConfigAsset : ScriptableObject
    {
        
        [Tooltip("Unity Input axis name for joystick horizontal.")]
        [field: SerializeField] public string HorizontalAxisName { get; set; }
        
        [Tooltip("Unity Input axis name for joystick vertical.")]
        [field: SerializeField] public string VerticalAxisName { get; set; }
        
        
        [field: SerializeField] public float DeadZone { get; set; }
        
        [field: SerializeField] public bool InvertX { get; set; } = false;
        [field: SerializeField] public bool InvertY { get; set; } = true;
    }
}
