using UnityEngine;

namespace Luxodd.Game.Example.Scripts.ControlTest
{
    /// <summary>
    /// Simple runtime state for the example scene (inventory / interaction context).
    /// </summary>
    public sealed class PlayerContext : MonoBehaviour
    {
        [field: SerializeField] public bool HasKey { get; private set; }
        [SerializeField] private PlayerControlBehaviour _playerControl;

        private void Awake()
        {
            _playerControl = GetComponent<PlayerControlBehaviour>();
        }

        public void GiveKey()
        {
            HasKey = true;
            Debug.Log("[ControlTest] Key picked up.");
        }

        public void ConsumeKey()
        {
            HasKey = false;
            Debug.Log("[ControlTest] Key used.");
        }
    }
}