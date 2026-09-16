using System.Collections.Generic;
using UnityEngine;

namespace JogoBruxinha.Gameplay.Inventory
{
    [CreateAssetMenu(fileName = "NewInventory", menuName = "Scriptable Objects/Game Data/Inventory")]
    public sealed class InventoryDataSO : ScriptableObject, ISerializationCallbackReceiver
    {
        public Dictionary<ItemDataSO, int> savedInv = new Dictionary<ItemDataSO, int>();

        [Header("Brewed Potions")]
        public List<BrewedPotion> craftedPotions = new List<BrewedPotion>();
        public int maxPotionSlots = 1;

        [Header("Inspector Serialization Helper")]
        [SerializeField] private List<ItemDataSO> _keys = new List<ItemDataSO>();
        [SerializeField] private List<int> _values = new List<int>();

        public void OnBeforeSerialize()
        {
            _keys.Clear();
            _values.Clear();

            foreach (KeyValuePair<ItemDataSO, int> pair in savedInv)
            {
                _keys.Add(pair.Key);
                _values.Add(pair.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            savedInv.Clear();

            if (_keys.Count != _values.Count) return;

            for (int i = 0; i < _keys.Count; i++)
            {
                if (_keys[i] != null && !savedInv.ContainsKey(_keys[i]))
                {
                    savedInv.Add(_keys[i], _values[i]);
                }
            }
        }

        public bool HasSpaceForPotion()
        {
            return craftedPotions.Count < maxPotionSlots;
        }

        public void AddPotion(BrewedPotion newPotion)
        {
            if (newPotion.potionData == null || !HasSpaceForPotion()) return;

            craftedPotions.Add(newPotion);

            if (savedInv.ContainsKey(newPotion.potionData))
            {
                savedInv[newPotion.potionData]++;
            }
            else
            {
                savedInv.Add(newPotion.potionData, 1);
            }
        }

        [ContextMenu("Reset Inventory")]
        public void ClearData()
        {
            savedInv.Clear();
            _keys.Clear();
            _values.Clear();
            craftedPotions.Clear();
        }
    }
}