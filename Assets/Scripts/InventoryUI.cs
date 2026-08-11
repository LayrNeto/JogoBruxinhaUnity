using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("Grid Configs")]
    public Transform gridParent;   
    public GameObject slotPrefab;   

    [Header("Audio")]
    public SoundDataSO openInventorySound;

    private PlayerInventory playerInv;

    void Awake()
    {
        playerInv = FindAnyObjectByType<PlayerInventory>();
    }

    void OnEnable()
    {
        RefreshInventoryUI();
        GameStateManager.Instance.PushState(GameStateManager.GameState.INVENTORY);
        AudioManager.Instance.PlaySFX(openInventorySound);
    }

    void OnDisable()
    {
        if (GameStateManager.Instance) GameStateManager.Instance.PopState();
    }


    public void RefreshInventoryUI()
    {
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }

        if (!playerInv) return;

        Dictionary<ItemDataSO, int> currentItems = playerInv.GetInventory();

        foreach (KeyValuePair<ItemDataSO, int> pair in currentItems)
        {
            if (pair.Value <= 0) continue;

            GameObject newSlot = Instantiate(slotPrefab, gridParent);

            InventorySlotUI slotScript = newSlot.GetComponent<InventorySlotUI>();
            slotScript.SetupSlot(pair.Key, pair.Value);
        }
    }
}