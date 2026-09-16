using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JogoBruxinha.Gameplay.Inventory
{
    public sealed class InventorySlotUI : MonoBehaviour
    {
        [Header("Visuals")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _amountText;

        private void Awake()
        {
            // Debug.Assert(_iconImage != null, $"Missing _iconImage reference on {name}", this);
            // Debug.Assert(_amountText != null, $"Missing _amountText reference on {name}", this);
        }

        public void SetupSlot(ItemDataSO itemData, int amount)
        {
            if (itemData == null) return;

            if (_iconImage != null)
            {
                _iconImage.sprite = itemData.inventoryIcon;
            }

            if (_amountText != null)
            {
                _amountText.text = $"x{amount}";
            }
        }
    }
}