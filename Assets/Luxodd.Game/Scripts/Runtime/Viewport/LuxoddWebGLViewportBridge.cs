using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Luxodd.Game.Scripts.Runtime.Viewport
{
    /// <summary>
    /// Optional WebGL helper: listens to browser resize/orientationchange and triggers UI/camera refresh.
    /// Use together with LuxoddViewportWatcher or standalone (it still calls ResetAspect + ForceUpdateCanvases).
    /// </summary>
    public sealed class LuxoddWebGLViewportBridge : MonoBehaviour
    {
        
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern int Luxodd_RegisterViewportChangedCallback(string goName, string methodName);
#endif

        [SerializeField] private Camera _camera;

        private void Awake()
        {
            if (_camera == null)
                _camera = Camera.main;
        }

        private void Start()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            Luxodd_RegisterViewportChangedCallback(gameObject.name, nameof(OnJsViewportChanged));
#endif
            ApplyNow();
        }

        // Called from JS via SendMessage(gameObject.name, "OnJsViewportChanged", "")
        private void OnJsViewportChanged(string _)
        {
            // Apply immediately + next frame (because Screen.width/height may settle with delay)
            ApplyNow();
            StartCoroutine(ApplyNextFrame());
        }

        private IEnumerator ApplyNextFrame()
        {
            yield return null;
            ApplyNow();
        }

        private void ApplyNow()
        {
            if (_camera != null)
                _camera.ResetAspect();

            Canvas.ForceUpdateCanvases();
        }
    }
}