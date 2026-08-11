using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PopupManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject popupPanel; 
    public Image iconImage;
    public TextMeshProUGUI nameText;

    [Header("Audio")]
    public SoundDataSO showPopupSound;

    [Header("Settings")]
    public float displayTime = 2.5f; 

    public void ShowPopup(ItemStruct itemData)
    {
        iconImage.sprite = itemData.item.inventoryIcon; 
        nameText.text = itemData.item.displayName;
        popupPanel.SetActive(true);

        AudioManager.Instance.PlaySFX(showPopupSound);

        StopAllCoroutines();
        StartCoroutine(HidePopupRoutine());
    }

    private IEnumerator HidePopupRoutine()
    {
        yield return new WaitForSeconds(displayTime);
        popupPanel.SetActive(false);
    }
}