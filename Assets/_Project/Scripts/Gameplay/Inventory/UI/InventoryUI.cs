using System.Collections.Generic;
using JogoBruxinha.Core.Audio;
using JogoBruxinha.Gameplay.Characters.Player;
using JogoBruxinha.Gameplay.GameFlow;
using UnityEngine;

namespace JogoBruxinha.Gameplay.Inventory
{
    public sealed class InventoryUI : MonoBehaviour
    {
        [Header("Grid Configs")]
        [SerializeField] private Transform _gridParent;
        [SerializeField] private GameObject _slotPrefab;

        [Header("Audio")]
        [SerializeField] private SoundDataSO _openInventorySound;

        [Header("Optional Manual Reference")]
        [SerializeField] private PlayerInventory _playerInv;

        private void Awake()
        {
            Debug.Assert(_gridParent != null, $"Missing _gridParent reference on {name}", this);
            Debug.Assert(_slotPrefab != null, $"Missing _slotPrefab reference on {name}", this);

            if (_playerInv == null)
            {
                _playerInv = FindAnyObjectByType<PlayerInventory>();
            }
        }

        private void OnEnable()
        {
            RefreshInventoryUI();

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.PushState(GameStateManager.GameState.INVENTORY);
            }

            if (AudioManager.Instance != null && _openInventorySound != null)
            {
                AudioManager.Instance.PlaySFX(_openInventorySound);
            }
        }

        private void OnDisable()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.PopState();
            }
        }

        public void RefreshInventoryUI()
        {
            if (_gridParent == null || _slotPrefab == null) return;

            foreach (Transform child in _gridParent)
            {
                Destroy(child.gameObject);
            }

            if (_playerInv == null) return;

            Dictionary<ItemDataSO, int> currentItems = _playerInv.GetInventory();
            if (currentItems == null) return;

            foreach (KeyValuePair<ItemDataSO, int> pair in currentItems)
            {
                if (pair.Key == null || pair.Value <= 0) continue;

                GameObject newSlot = Instantiate(_slotPrefab, _gridParent);
                InventorySlotUI slotScript = newSlot.GetComponent<InventorySlotUI>();

                if (slotScript != null)
                {
                    slotScript.SetupSlot(pair.Key, pair.Value);
                }
            }
        }
    }
}