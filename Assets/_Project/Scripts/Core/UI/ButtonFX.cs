using JogoBruxinha.Core.Audio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JogoBruxinha.Core.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class ButtonFX : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
    {
        [Header("Audio")]
        [SerializeField] private SoundDataSO _hoverSound;
        [SerializeField] private SoundDataSO _clickSound;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            Debug.Assert(_button != null, $"Missing Button component on {name}", this);
            Debug.Assert(_hoverSound != null, $"Missing hoverSound reference on {name}", this);
            Debug.Assert(_clickSound != null, $"Missing clickSound reference on {name}", this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_button.interactable) return;

            if (AudioManager.Instance != null && _hoverSound != null)
                AudioManager.Instance.PlaySFX(_hoverSound);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_button.interactable) return;

            if (AudioManager.Instance != null && _clickSound != null)
                AudioManager.Instance.PlaySFX(_clickSound);
        }

    }
}