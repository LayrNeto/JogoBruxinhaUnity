using UnityEngine;

public class IngredientManager : MonoBehaviour
{
    [Header("Inventory Data")]
    public InventoryDataSO inventory;

    [Header("Ingredients POV")]
    public IngredientSlot[] slots; 

    void Start()
    {
        UpdateShelves();
    }

    public void UpdateShelves()
    {
        if (inventory == null || slots == null) return;

        foreach (IngredientSlot slot in slots)
        {
            if (!slot.plantData) continue;

            int qnt = 0;

            if (inventory.savedInv.TryGetValue(slot.plantData, out int savedQnt))
            {
                qnt = savedQnt;
            }

            if (slot.amount)
            {
                slot.amount.text = "x" + qnt;
            }

            slot.gameObject.SetActive(qnt > 0);
        }
    }
}