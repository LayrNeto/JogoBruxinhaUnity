using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class InventorySlotUI : MonoBehaviour
{
    [Header("Visuals")]
    public Image iconImage;
    public TextMeshProUGUI amountText;

    public void SetupSlot(ItemDataSO itemData, int amount)
    {
        iconImage.sprite = itemData.inventoryIcon;
        amountText.text = "x" + amount.ToString();
    }
}