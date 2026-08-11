using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonFX : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("Áudio")]
    public SoundDataSO hoverSound;
    public SoundDataSO clickSound;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable) return;

        if (AudioManager.Instance != null && hoverSound != null)
            AudioManager.Instance.PlaySFX(hoverSound);

        // CursorManager.Instance.SetCursor(CursorType.Hover);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.interactable) return;

        if (AudioManager.Instance != null && clickSound != null)
            AudioManager.Instance.PlaySFX(clickSound);

        // CursorManager.Instance.SetCursor(CursorType.Click);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // CursorManager.Instance.SetCursor(CursorType.Hover);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // CursorManager.Instance.SetCursor(CursorType.Normal);
    }
}