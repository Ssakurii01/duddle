using UnityEngine;

namespace Luxodd.Game.Scripts.Input
{
    public class ArcadeInputSetup : MonoBehaviour
    {
        [SerializeField] private ArcadeInputConfigAsset _inputConfigAsset;

        private void Awake()
        {
            ArcadeControls.Config = _inputConfigAsset;
        }
    }
}
