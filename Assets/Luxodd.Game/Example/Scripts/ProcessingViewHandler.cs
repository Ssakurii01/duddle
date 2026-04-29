using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Luxodd.Game.Example.Scripts
{
    public class ProcessingViewHandler : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private float _rotationSpeed = 5f;

        private bool _isShowing = false;
        private Coroutine _coroutine;
        private float _currentRotation = 0f;
        
        public void Show()
        {
            gameObject.SetActive(true);
            _isShowing = true;
            _coroutine = StartCoroutine(StartRotation());
        }

        public void Hide()
        {
            _isShowing = false;
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
            }
            gameObject.SetActive(false);
        }

        private void Awake()
        {
            Hide();
        }

        [ContextMenu("Show")]
        private void TestShowing()
        {
            Show();
        }

        [ContextMenu("Hide")]
        private void TestHide()
        {
            Hide();
        }

        private IEnumerator StartRotation()
        {
            while (_isShowing)
            {
                _currentRotation += _rotationSpeed * Time.deltaTime;
                _image.transform.localRotation = Quaternion.Euler(0f, 0f, _currentRotation);
                yield return null;
            }
        }
    }
}
