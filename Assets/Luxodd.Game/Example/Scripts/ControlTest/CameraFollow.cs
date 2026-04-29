using UnityEngine;

namespace Luxodd.Game.Example.Scripts.ControlTest
{
    /// <summary>
    /// Simple camera follow logic for 2D scenes.
    /// Keeps the camera centered on the player with optional smoothing.
    /// </summary>
    public sealed class CameraFollow : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform _target;

        [Header("Offset")]
        [SerializeField] private Vector3 _offset = new Vector3(0f, 0f, -10f);

        [Header("Smoothing")]
        [SerializeField] private float _followSpeed = 8f;

        private void LateUpdate()
        {
            if (_target == null)
                return;

            var desiredPosition = _target.position + _offset;

            transform.position = Vector3.Lerp(
                transform.position,
                desiredPosition,
                _followSpeed * Time.deltaTime
            );
        }
    }
}