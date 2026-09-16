using System.Collections.Generic;
using JogoBruxinha.Core.Events;
using JogoBruxinha.Gameplay.GameFlow;
using JogoBruxinha.Gameplay.Inventory;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JogoBruxinha.Gameplay.Characters.Player
{
    public sealed class PlayerInventory : MonoBehaviour
    {
        [Header("Events")]
        [SerializeField] private GameEvent _toggleInventoryEvent;

        [Header("Inventory SO")]
        [SerializeField] private InventoryDataSO _inventorySO;

        public InventoryDataSO InventorySO => _inventorySO;

        private void Awake()
        {
            Debug.Assert(_toggleInventoryEvent != null, $"Missing _toggleInventoryEvent reference on {name}", this);
            Debug.Assert(_inventorySO != null, $"Missing _inventorySO reference on {name}", this);
        }

        private void Start()
        {
            if (GameStateManager.Instance != null && GameStateManager.Instance.InputControls != null)
            {
                GameStateManager.Instance.InputControls.Global.ToggleInventory.performed += ShowInventory;
            }
        }

        private void OnDestroy()
        {
            if (GameStateManager.Instance != null && GameStateManager.Instance.InputControls != null)
            {
                GameStateManager.Instance.InputControls.Global.ToggleInventory.performed -= ShowInventory;
            }
        }

        private void ShowInventory(InputAction.CallbackContext context)
        {
            _toggleInventoryEvent?.Raise();
        }

        public void AddItem(ItemStruct itemStruct)
        {
            if (_inventorySO == null || itemStruct.item == null) return;

            if (!_inventorySO.savedInv.ContainsKey(itemStruct.item))
            {
                _inventorySO.savedInv.Add(itemStruct.item, 0);
            }

            _inventorySO.savedInv[itemStruct.item] += itemStruct.amount;

            Debug.Log($"Inventory: {itemStruct.item.displayName} now has {_inventorySO.savedInv[itemStruct.item]} units.");
        }

        public Dictionary<ItemDataSO, int> GetInventory()
        {
            return _inventorySO != null ? _inventorySO.savedInv : null;
        }
    }
}