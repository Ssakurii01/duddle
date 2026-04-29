using UnityEngine;

namespace Luxodd.Game.Example.Scripts.ControlTest
{
    /// <summary>
    /// Trigger zone that allows the player to move vertically (ladder behavior).
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class LadderZone : MonoBehaviour
    {
        private void Reset()
        {
            var col = GetComponent<BoxCollider2D>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponentInParent<PlayerControlBehaviour>();
            if (player != null)
            {
                player.SetNearLadder(true);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var player = other.GetComponentInParent<PlayerControlBehaviour>();
            if (player != null)
            {
                player.SetNearLadder(false);
            }
        }
    }
}