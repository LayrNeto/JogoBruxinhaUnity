using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewInventory", menuName = "Scriptable Objects/Game Data/Inventory")]
public class InventoryDataSO : ScriptableObject, ISerializationCallbackReceiver
{
    public Dictionary<ItemDataSO, int> savedInv = new Dictionary<ItemDataSO, int>();

    [Header("Inventory [Read Only]")]
    public List<ItemDataSO> keys = new List<ItemDataSO>();
    public List<int> values = new List<int>();

    [Header("Brewed Potions")]
    public List<BrewedPotion> craftedPotions = new List<BrewedPotion>();
    public int maxPotionSlots = 1;


    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();

        foreach (KeyValuePair<ItemDataSO, int> pair in savedInv)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }


    public void OnAfterDeserialize()
    {
        savedInv.Clear();

        if (keys.Count != values.Count) return;

        for (int i = 0; i < keys.Count; i++)
        {
            savedInv.Add(keys[i], values[i]);
        }
    }

    public bool HasSpaceForPotion()
    {
        return craftedPotions.Count < maxPotionSlots;
    }

    public void AddPotion(BrewedPotion newPotion)
    {
        if (HasSpaceForPotion())
        {
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
    }

    [ContextMenu("Reset Inventory")]
    public void ClearData()
    {
        savedInv.Clear();
        keys.Clear();
        values.Clear();
        craftedPotions.Clear();
    }
}