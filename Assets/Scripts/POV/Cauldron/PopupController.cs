using System.Collections;
using UnityEngine;
using TMPro;

public class PopupController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI popupText;
    
    [Header("Settings")]
    public float displayDuration = 3f; 

    public void ShowMessage(string message)
    {
        popupText.text = message;
        popupText.gameObject.SetActive(true);

        StopAllCoroutines(); 
        StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        
        popupText.gameObject.SetActive(false);
    }
}