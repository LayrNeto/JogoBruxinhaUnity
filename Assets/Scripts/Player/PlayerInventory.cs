using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInventory : MonoBehaviour
{
    [Header("Events")]
    public GameEvent toggleInventoryEvent;

    [Header("Inventory SO")]
    public InventoryDataSO inventorySO;

    void Start()
    {
        GameStateManager.Instance.inputControls.Global.ToggleInventory.performed += ShowInventory;
    }

    void OnDestroy()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.inputControls.Global.ToggleInventory.performed -= ShowInventory;
        }
    }

    private void ShowInventory(InputAction.CallbackContext context)
    {
        toggleInventoryEvent?.Raise();
    }

    public void AddItem(ItemStruct itemStruct)
    {
        if (!inventorySO.savedInv.ContainsKey(itemStruct.item))
        {
            inventorySO.savedInv.Add(itemStruct.item, 0);
        }       
        inventorySO.savedInv[itemStruct.item] += itemStruct.amount;
        
        Debug.Log($"Inventory: {itemStruct.item.displayName} now has {inventorySO.savedInv[itemStruct.item]} unitys.");
    }

    public Dictionary<ItemDataSO, int> GetInventory()
    {
        return inventorySO.savedInv;
    }
}