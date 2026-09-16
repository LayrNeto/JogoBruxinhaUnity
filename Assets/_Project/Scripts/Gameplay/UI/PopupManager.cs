using System.Collections;
using JogoBruxinha.Core.Audio;
using JogoBruxinha.Gameplay.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JogoBruxinha.Gameplay.UI
{
    public sealed class PopupManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _popupPanel;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _nameText;

        [Header("Audio")]
        [SerializeField] private SoundDataSO _showPopupSound;

        [Header("Settings")]
        [SerializeField] private float _displayTime = 2.5f;

        private Coroutine _hideCoroutine;

        private void Awake()
        {
            Debug.Assert(_popupPanel != null, $"Missing _popupPanel reference on {name}", this);
            Debug.Assert(_iconImage != null, $"Missing _iconImage reference on {name}", this);
            Debug.Assert(_nameText != null, $"Missing _nameText reference on {name}", this);
        }

        private void OnDisable()
        {
            if (_hideCoroutine != null)
            {
                StopCoroutine(_hideCoroutine);
                _hideCoroutine = null;
            }
        }

        public void ShowPopup(ItemStruct itemData)
        {
            if (itemData.item == null || _popupPanel == null) return;

            if (_iconImage != null)
            {
                _iconImage.sprite = itemData.item.inventoryIcon;
            }

            if (_nameText != null)
            {
                _nameText.text = itemData.item.displayName;
            }

            _popupPanel.SetActive(true);

            if (AudioManager.Instance != null && _showPopupSound != null)
            {
                AudioManager.Instance.PlaySFX(_showPopupSound);
            }

            if (_hideCoroutine != null)
            {
                StopCoroutine(_hideCoroutine);
            }

            _hideCoroutine = StartCoroutine(HidePopupRoutine());
        }

        private IEnumerator HidePopupRoutine()
        {
            yield return new WaitForSeconds(_displayTime);

            if (_popupPanel != null)
            {
                _popupPanel.SetActive(false);
            }

            _hideCoroutine = null;
        }
    }
}