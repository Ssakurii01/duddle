using System.Collections.Generic;
using UnityEngine;

namespace Luxodd.Game.Scripts.Network
{
    public enum ServerEnvironment
    {
        Staging,
        Production,
    }
    
    [CreateAssetMenu(menuName = "Unity Plugin/Network/Settings Descriptor", fileName = "NetworkSettingsDescriptor", order = 0)]
    public class NetworkSettingsDescriptor : ScriptableObject
    {
        [field:SerializeField] public string ServerAddress { get; private set; }
        [field: SerializeField] public string DeveloperDebugToken { get; set; }
    }
}
