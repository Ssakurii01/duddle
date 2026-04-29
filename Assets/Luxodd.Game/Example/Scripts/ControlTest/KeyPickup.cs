using UnityEngine;

namespace Luxodd.Game.Example.Scripts.ControlTest
{
    /// <summary>
    /// Key object: when the player enters the trigger, the key is granted and object is destroyed.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class KeyPickup : MonoBehaviour
    {
        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var context = other.GetComponentInParent<PlayerContext>();
            if (context == null)
                return;

            context.GiveKey();
            Destroy(gameObject);
        }
    }
}