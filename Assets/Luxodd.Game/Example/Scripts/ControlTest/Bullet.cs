using UnityEngine;

namespace Luxodd.Game.Example.Scripts.ControlTest
{
    /// <summary>
    /// Minimal bullet: moves in a straight line and destroys itself after lifetime.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class Bullet : MonoBehaviour
    {
        [SerializeField] private float _lifeTime = 2f;

        private Vector2 _velocity;

        public void Launch(Vector2 direction, float speed)
        {
            _velocity = direction.normalized * speed;
            Destroy(gameObject, _lifeTime);
        }

        private void Update()
        {
            transform.position += (Vector3)(_velocity * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // For test scene: destroy on any trigger hit (optional).
            // You can disable this if you want bullets to pass through triggers.
            if (!other.isTrigger)
            {
                Destroy(gameObject);
            }
        }
    }
}