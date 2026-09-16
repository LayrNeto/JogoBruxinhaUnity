using JogoBruxinha.Gameplay.Inventory;
using UnityEngine;

namespace JogoBruxinha.Gameplay.Cauldron
{
    public sealed class IngredientManager : MonoBehaviour
    {
        [Header("Inventory Data")]
        [SerializeField] private InventoryDataSO _inventory;

        [Header("Ingredients POV")]
        [SerializeField] private IngredientSlot[] _slots;

        private void Awake()
        {
            Debug.Assert(_inventory != null, $"Missing _inventory reference on {name}", this);
            Debug.Assert(_slots != null && _slots.Length > 0, $"Missing or empty _slots array on {name}", this);
        }

        private void Start()
        {
            UpdateShelves();
        }

        public void UpdateShelves()
        {
            if (_inventory == null || _slots == null) return;

            foreach (IngredientSlot slot in _slots)
            {
                if (slot == null || slot.PlantData == null) continue;

                int qnt = 0;

                if (_inventory.savedInv.TryGetValue(slot.PlantData, out int savedQnt))
                {
                    qnt = savedQnt;
                }

                if (slot.Amount != null)
                {
                    slot.Amount.text = $"x{qnt}";
                }

                slot.gameObject.SetActive(qnt > 0);
            }
        }
    }
}