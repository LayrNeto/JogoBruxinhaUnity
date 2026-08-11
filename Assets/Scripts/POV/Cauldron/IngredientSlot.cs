using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; 
using TMPro; 

public class IngredientSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    public Image backgroundImg; 
    public Image foregroundImg; 
    public TextMeshProUGUI amount;

    [Header("Ingredient Data")]
    public PlantDataSO plantData; 
    
    private GameObject ghostIcon;
    private Canvas cauldronCanvas;

    void Start()
    {
        cauldronCanvas = GetComponentInParent<Canvas>();

        if (plantData != null)
        {
            foregroundImg.sprite = plantData.shelfSprite;
            
            if (plantData.emptyShelfSprite != null)
            {
                backgroundImg.gameObject.SetActive(true);
                backgroundImg.sprite = plantData.emptyShelfSprite;
            }
            else
            {
                backgroundImg.gameObject.SetActive(false);
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        foregroundImg.enabled = false;
        amount.enabled = false;

        ghostIcon = new GameObject("ghostIcon");
        ghostIcon.transform.SetParent(cauldronCanvas.transform, false);

        Image img = ghostIcon.AddComponent<Image>();
        img.sprite = plantData.dragSprite;
        img.raycastTarget = false; 
        img.SetNativeSize();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ghostIcon)
        {
            ghostIcon.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (ghostIcon != null)
        {
            Destroy(ghostIcon);
        }

        foregroundImg.enabled = true;
        amount.enabled = true;
    }
}