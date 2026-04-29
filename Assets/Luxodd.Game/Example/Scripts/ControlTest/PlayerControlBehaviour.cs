using System;
using UnityEngine;

namespace Luxodd.Game.Example.Scripts.ControlTest
{
    /// <summary>
    /// Applies gameplay logic based on input adapter: movement, ladder, jump, shooting and item usage.
    /// </summary>
    public sealed class PlayerControlBehaviour : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Rigidbody2D _playerRigidbody;
        
        [SerializeField] private SpriteRenderer _spriteRenderer;

        [SerializeField] private Transform _groundCheck;
        [SerializeField] private LayerMask _groundMask;

        [SerializeField] private Transform _muzzle;
        [SerializeField] private Bullet _bulletPrefab;

        [Header("Movement")]
        [SerializeField] private float _movementSpeed = 7f;

        [Header("Ladder")]
        [SerializeField] private float _ladderSpeed = 5f;
        [SerializeField] private float _ladderInputDeadZone = 0.2f;

        [Header("Jump")]
        [SerializeField] private float _jumpForce = 12f;
        [SerializeField] private float _groundCheckRadius = 0.12f;

        [Header("Weapon")]
        [SerializeField] private float _fireRate = 3f; // shots per second
        [SerializeField] private float _bulletSpeed = 12f;
        
        [SerializeField] private float _facingDeadZone = 0.2f;

        private IPlayerControlAdapter _control;
        private PlayerContext _context;

        private bool _isNearLadder;
        private bool _isOnLadder;

        private bool _jumpRequested;
        private float _nextFireTime;

        private int _facingSign = 1; // 1 = right, -1 = left
        private Door _nearDoor;

        private float _defaultGravityScale;

        public void SetControlAdapter(IPlayerControlAdapter adapter)
        {
            Unsubscribe();
            _control = adapter;
            Subscribe();
        }

        public void SetNearLadder(bool isNear)
        {
            _isNearLadder = isNear;

            if (!_isNearLadder && _isOnLadder)
            {
                ExitLadder();
            }
        }

        public void SetNearDoor(Door door)
        {
            _nearDoor = door;
        }

        private void Awake()
        {
            if (_playerRigidbody == null)
            {
                _playerRigidbody = GetComponent<Rigidbody2D>();
            }

            _context = GetComponent<PlayerContext>();
            _defaultGravityScale = _playerRigidbody.gravityScale;

            // Auto-pick adapter on the same GameObject.
            var adapter = GetComponent<IPlayerControlAdapter>();
            if (adapter != null)
            {
                SetControlAdapter(adapter);
            }
            
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable() => Subscribe();
        private void OnDisable() => Unsubscribe();

        private void Update()
        {
            if (_control == null)
                return;

            var move = _control.MovementVector;

            UpdateFacing(move.x);
            UpdateLadderState(move);
            HandleJumpRequest();
            HandleFire();
        }

        private void FixedUpdate()
        {
            if (_control == null)
                return;

            var move = _control.MovementVector;

            if (_isOnLadder)
            {
                var vx = move.x * _movementSpeed;
                var vy = move.y * _ladderSpeed;
                _playerRigidbody.linearVelocity = new Vector2(vx, vy);
                return;
            }

            var targetVx = move.x * _movementSpeed;
            _playerRigidbody.linearVelocity = new Vector2(targetVx, _playerRigidbody.linearVelocity.y);
        }

        private void UpdateFacing(float x)
        {
            
            if (x >= _facingDeadZone)
            {
                SetFacing(1);
            }
            else if (x <= -_facingDeadZone)
            {
                SetFacing(-1);
            }
            
            //Debug.Log($"[{DateTime.Now}][{GetType().Name}][{nameof(UpdateFacing)}] OK, x: {x}, y: {_facingSign}");
        }
        
        private void SetFacing(int sign)
        {
            if (sign == _facingSign)
                return;

            _facingSign = sign;
            _spriteRenderer.flipX = (_facingSign < 0);
            
            var localScale = transform.localScale;
            localScale.x = _facingSign;
            transform.localScale = localScale;
        }

        private void UpdateLadderState(Vector2 move)
        {
            if (!_isOnLadder)
            {
                if (_isNearLadder && Mathf.Abs(move.y) >= _ladderInputDeadZone)
                {
                    EnterLadder();
                }

                return;
            }

            if (!_isNearLadder)
            {
                ExitLadder();
            }
        }

        private void EnterLadder()
        {
            _isOnLadder = true;
            _playerRigidbody.gravityScale = 0f;
            _playerRigidbody.linearVelocity = Vector2.zero;
        }

        private void ExitLadder()
        {
            _isOnLadder = false;
            _playerRigidbody.gravityScale = _defaultGravityScale;
        }

        private void HandleJumpRequest()
        {
            if (!_jumpRequested)
                return;

            _jumpRequested = false;

            if (_isOnLadder)
            {
                ExitLadder();
            }

            if (!IsGrounded())
                return;

            _playerRigidbody.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
        }

        private bool IsGrounded()
        {
            if (_groundCheck == null)
                return false;

            return Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundMask) != null;
        }

        private void HandleFire()
        {
            if (_bulletPrefab == null || _muzzle == null)
                return;

            if (!_control.IsFireButtonPressed)
                return;

            var now = Time.time;
            if (now < _nextFireTime)
                return;

            _nextFireTime = now + (1f / _fireRate);

            var dir = new Vector2(_facingSign, 0f);
            var bullet = Instantiate(_bulletPrefab, _muzzle.position, Quaternion.identity);
            bullet.Launch(dir, _bulletSpeed);
        }

        private void Subscribe()
        {
            if (_control == null)
                return;

            _control.JumpButtonPressed += OnJumpPressed;
            _control.UseItemButtonPressed += OnUseItemPressed;
        }

        private void Unsubscribe()
        {
            if (_control == null)
                return;

            _control.JumpButtonPressed -= OnJumpPressed;
            _control.UseItemButtonPressed -= OnUseItemPressed;
        }

        private void OnJumpPressed()
        {
            _jumpRequested = true;
        }

        private void OnUseItemPressed()
        {
            if (_nearDoor == null)
            {
                Debug.Log("[ControlTest] Use pressed, but no door nearby.");
                return;
            }

            if (_context == null || !_context.HasKey)
            {
                Debug.Log("[ControlTest] Need a key to open the door.");
                return;
            }

            if (_nearDoor.IsLocked)
            {
                _nearDoor.UnlockAndOpen();
                _context.ConsumeKey();
                return;
            }

            _nearDoor.Open();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_groundCheck == null)
                return;

            Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
        }
#endif
    }
}
