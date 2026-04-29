using UnityEngine;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace Luxodd.Game
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EventSystem))]
    public class EventSystemInputModuleSwitcher : MonoBehaviour
    {
        [Tooltip("If true, will auto-fix the input module in Editor via OnValidate.")]
        public bool autoFixInEditor = true;

        private void Awake()
        {
            EnsureCorrectModule();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!autoFixInEditor) return;
            
            EnsureCorrectModule();
        }
#endif

        private void EnsureCorrectModule()
        {
#if ENABLE_INPUT_SYSTEM
            
            var newModule = GetComponent<InputSystemUIInputModule>();
            if (newModule == null)
                newModule = gameObject.AddComponent<InputSystemUIInputModule>();

           
            var oldModule = GetComponent<StandaloneInputModule>();
            if (oldModule != null)
                DestroyImmediateSafe(oldModule);

#else
          
            var oldModule = GetComponent<StandaloneInputModule>();
            if (oldModule == null)
                oldModule = gameObject.AddComponent<StandaloneInputModule>();

            
            var newModule = GetComponent("InputSystemUIInputModule");
            if (newModule != null)
                DestroyImmediateSafe((Component)newModule);
#endif
        }

        private static void DestroyImmediateSafe(Component c)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) Object.DestroyImmediate(c);
            else Object.Destroy(c);
#else
            Object.Destroy(c);
#endif
        }
    }
}
