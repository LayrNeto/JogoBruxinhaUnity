using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CauldronController : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    [Header("Visual Components")]
    public Animator cauldronAnimator;
    public Image[] floatingIcons; 
    public PopupController popup;
    public IngredientManager ingredientManager;
    public GameObject spellFilter;

    [Header("Potion Visual Tokens")]
    public PotionDataSO normalPotionVisual; 
    public PotionDataSO magicPotionVisual;

    [Header("Game Data")]
    public SessionDataSO sessionData; 
    public InventoryDataSO inventoryData;

    [Header("Audio")]
    public SoundDataSO spellCastedSound;
    public SoundDataSO errorSound;
    public SoundDataSO ingredientAddedSound;
    public SoundDataSO potionPreparedSound;

    [Header("Delegates/Hooks")]
    public Func<PlantDataSO, bool> onValidateIngredientDrop;
    public Func<bool> onValidateCauldronClick;
    public Action<int> onIngredientAdded;
    public Action<bool> onPotionFinished;

    private HashSet<PlantDataSO> currentIngredients = new HashSet<PlantDataSO>();
    private readonly int maxIngredients = 3;

    private void OnEnable()
    {
        if (!popup) Debug.Log("Missing popup reference on cauldron canvas!");

        if (sessionData && sessionData.potionAwaitingDelivery)
        {
            if (cauldronAnimator)
            {
                bool wasSpellUsed = false;

                if (inventoryData && inventoryData.craftedPotions.Count > 0)
                {
                    wasSpellUsed = inventoryData.craftedPotions[0].isSpellUsed; 
                }

                cauldronAnimator.SetBool("PotionReady", true);
                cauldronAnimator.SetBool("SpellUsed", wasSpellUsed);

                string stateName = wasSpellUsed ? "cauldronSpellUsed" : "cauldronPotionFinished"; 
                cauldronAnimator.Play(stateName); 
            }
        }
        else
        {
            currentIngredients.Clear();
            ResetVisuals();
        }
            popup.gameObject.SetActive(false);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (sessionData && sessionData.potionAwaitingDelivery)
        {
            ShowErrorMessage("Já existe uma poção no balcão");
            return;
        }

        IngredientSlot draggedSlot = eventData.pointerDrag.GetComponent<IngredientSlot>();
        
        if (draggedSlot)
        {
            if (onValidateIngredientDrop != null && !onValidateIngredientDrop.Invoke(draggedSlot.plantData))
            {
                return; 
            }

            if (currentIngredients.Count < maxIngredients)
            {
                if (currentIngredients.Add(draggedSlot.plantData))
                {
                    AudioManager.Instance.PlaySFX(ingredientAddedSound);

                    bool hasBg = draggedSlot.backgroundImg.sprite != null ? true : false;
                    UpdateFloatingIcons(draggedSlot.plantData.dragSprite, hasBg);
                    
                    string message = (currentIngredients.Count == maxIngredients) ? "Clique no caldeirão para engarrafar a poção" : "Ingrediente adicionado";
                    popup.ShowMessage(message);

                    if (cauldronAnimator != null)
                    {
                        cauldronAnimator.SetBool("PotionStarted", true);
                    }

                    onIngredientAdded?.Invoke(currentIngredients.Count);
                }
                else
                {
                    ShowErrorMessage("Ingrediente repetido");
                }
            }
            else
            {
               ShowErrorMessage("O caldeirao já está cheio");
            }
        }
    }

    private void UpdateFloatingIcons(Sprite ingredientSprite, bool hasBg)
    {
        for (int i = 0; i < floatingIcons.Length; i++)
        {
            if (!floatingIcons[i].enabled)
            {
                if (hasBg)
                {
                    floatingIcons[i].gameObject.transform.localScale = new Vector3(1, 1, 0);
                }
                floatingIcons[i].sprite = ingredientSprite;
                floatingIcons[i].enabled = true;
                floatingIcons[i].SetNativeSize();
                break;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (sessionData != null && sessionData.potionAwaitingDelivery)
        {
            ShowErrorMessage("Leve primeiro a poção ao balcão");
            return;
        }

        if (onValidateCauldronClick != null && !onValidateCauldronClick.Invoke())
        {
            return; 
        }

        if (currentIngredients.Count == maxIngredients)
        {
            FinishPotion(false);
        }
        else
        {
            ShowErrorMessage($"Adicione {maxIngredients} ingredientes primeiro");
        }
    }

    public void BrewWithMagic()
    {
        if (sessionData != null && sessionData.potionAwaitingDelivery)
        {
            ShowErrorMessage("Leve primeiro a poção ao balcão");
            return;
        }

        if (currentIngredients.Count == maxIngredients)
        {
            AudioManager.Instance.PlaySFX(spellCastedSound);
            spellFilter.SetActive(true);
            FinishPotion(true);
        }
        else
        {
            ShowErrorMessage($"Adicione {maxIngredients} ingredientes primeiro");
        }
    }

    private void FinishPotion(bool isSpellUsed)
    {
        AudioManager.Instance.PlaySFX(potionPreparedSound);

        if (sessionData != null)
        {
            sessionData.potionAwaitingDelivery = true;
        }

        PotionDataSO visualToken = isSpellUsed ? magicPotionVisual : normalPotionVisual;
        BrewedPotion potion = new BrewedPotion(currentIngredients, isSpellUsed, visualToken);

        if (inventoryData && inventoryData.HasSpaceForPotion())
        {
            inventoryData.AddPotion(potion);

            foreach (ItemDataSO item in currentIngredients)
            {
                inventoryData.savedInv[item] -= 1;
            }
            ingredientManager.UpdateShelves();
        }

        currentIngredients.Clear();
        ResetVisuals();
        
        if (cauldronAnimator)
        {
            cauldronAnimator.SetBool("PotionReady", true);
            cauldronAnimator.SetBool("SpellUsed", isSpellUsed);
        }

        if (popup) popup.ShowMessage("Poção enviada para o inventário");

        onPotionFinished?.Invoke(isSpellUsed);
    }

    private void ResetVisuals()
    {
        foreach (Image icon in floatingIcons)
        {
            icon.enabled = false;
            icon.sprite = null;
        }
        if (cauldronAnimator != null)
        {
            cauldronAnimator.SetBool("PotionStarted", false);
        }
    }

    private void ShowErrorMessage(string popupText)
    {
        AudioManager.Instance.PlaySFX(errorSound);
        if (popup) popup.ShowMessage(popupText);
    }
}