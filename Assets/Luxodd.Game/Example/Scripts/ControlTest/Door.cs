using UnityEngine;

namespace Luxodd.Game.Example.Scripts.ControlTest
{
    /// <summary>
    /// Minimal door example: can be unlocked with a key and opened.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class Door : MonoBehaviour
    {
        [SerializeField] private bool _isLocked = true;
        [SerializeField] private SpriteRenderer _lockedDoorSprite;
        [SerializeField] private SpriteRenderer _unlockedDoorSprite;

        private Collider2D _collider;
        private SpriteRenderer _renderer;

        public bool IsLocked => _isLocked;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _renderer = GetComponent<SpriteRenderer>();
            UpdateVisual();
            SetDoorState(_isLocked);
        }

        public void UnlockAndOpen()
        {
            _isLocked = false;
            Open();
        }

        public void Open()
        {
            if (_collider != null)
            {
                _collider.enabled = false;
            }

            Debug.Log("[ControlTest] Door opened.");
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (_renderer == null)
                return;

            // Just a simple visual hint: locked = darker, opened = very transparent.
            if (_isLocked)
            {
                _renderer.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            }
            else
            {
                _renderer.color = new Color(0.6f, 1f, 0.6f, 0.25f);
            }
            
            SetDoorState(_isLocked);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponentInParent<PlayerControlBehaviour>();
            if (player != null)
            {
                player.SetNearDoor(this);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var player = other.GetComponentInParent<PlayerControlBehaviour>();
            if (player != null)
            {
                player.SetNearDoor(null);
            }
        }

        private void SetDoorState(bool isOpen)
        {
            _lockedDoorSprite.gameObject.SetActive(isOpen);
            _unlockedDoorSprite.gameObject.SetActive(!isOpen);
        }
    }
}