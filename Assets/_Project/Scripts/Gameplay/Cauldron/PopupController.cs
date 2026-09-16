using System.Collections;
using UnityEngine;
using TMPro;

namespace JogoBruxinha.Gameplay.Cauldron
{
    public sealed class PopupController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _popupText;

        [Header("Settings")]
        [SerializeField] private float _displayDuration = 3f;

        private Coroutine _hideCoroutine;

        private void Awake()
        {
            Debug.Assert(_popupText != null, $"Missing _popupText reference on {name}", this);
        }

        public void ShowMessage(string message)
        {
            if (_popupText == null) return;

            _popupText.text = message;
            _popupText.gameObject.SetActive(true);

            if (_hideCoroutine != null)
            {
                StopCoroutine(_hideCoroutine);
            }

            _hideCoroutine = StartCoroutine(HideAfterDelay());
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSeconds(_displayDuration);

            if (_popupText != null)
            {
                _popupText.gameObject.SetActive(false);
            }

            _hideCoroutine = null;
        }
    }
}