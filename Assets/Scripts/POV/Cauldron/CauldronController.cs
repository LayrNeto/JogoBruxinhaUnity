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
    private readonly int MaxIngredients = 3;

    private static readonly int PotionReadyHash = Animator.StringToHash("PotionReady");
    private static readonly int SpellUsedHash = Animator.StringToHash("SpellUsed");
    private static readonly int PotionStartedHash = Animator.StringToHash("PotionStarted");
    private static readonly int CauldronSpellUsedStateHash = Animator.StringToHash("cauldronSpellUsed");
    private static readonly int CauldronPotionFinishedStateHash = Animator.StringToHash("cauldronPotionFinished");

    private void OnEnable()
    {
        if (!popup) Debug.LogWarning("Missing popup reference on cauldron canvas!");

        if (sessionData && sessionData.potionAwaitingDelivery)
        {
            if (cauldronAnimator)
            {
                bool wasSpellUsed = false;

                if (inventoryData && inventoryData.craftedPotions.Count > 0)
                {
                    wasSpellUsed = inventoryData.craftedPotions[0].isSpellUsed; 
                }

                cauldronAnimator.SetBool(PotionReadyHash, true);
                cauldronAnimator.SetBool(SpellUsedHash, wasSpellUsed);

                int targetStateHash = wasSpellUsed ? CauldronSpellUsedStateHash : CauldronPotionFinishedStateHash;
                cauldronAnimator.Play(targetStateHash); 
            }
        }
        else
        {
            currentIngredients.Clear();
            ResetVisuals();
        }

        if (popup) popup.gameObject.SetActive(false);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (sessionData && sessionData.potionAwaitingDelivery)
        {
            ShowErrorMessage("Já existe uma poção no balcão");
            return;
        }

        if (eventData.pointerDrag == null) return;

        IngredientSlot draggedSlot = eventData.pointerDrag.GetComponent<IngredientSlot>();
        
        if (draggedSlot == null) return;

        if (onValidateIngredientDrop != null && !onValidateIngredientDrop.Invoke(draggedSlot.plantData)) return; 

        if (currentIngredients.Count >= MaxIngredients)
        {
            ShowErrorMessage("O caldeirao já está cheio");
            return;
        }

        if (currentIngredients.Add(draggedSlot.plantData))
        {
            AudioManager.Instance.PlaySFX(ingredientAddedSound);

            bool hasBg = draggedSlot.backgroundImg.sprite != null;
            UpdateFloatingIcons(draggedSlot.plantData.dragSprite, hasBg);
            
            string message = (currentIngredients.Count == MaxIngredients) ? "Clique no caldeirão para engarrafar a poção" : "Ingrediente adicionado";
            if (popup) popup.ShowMessage(message);

            if (cauldronAnimator != null)
                cauldronAnimator.SetBool("PotionStarted", true);

            onIngredientAdded?.Invoke(currentIngredients.Count);
        }
        else
        {
            ShowErrorMessage("Ingrediente repetido");
        }
    }

    private void UpdateFloatingIcons(Sprite ingredientSprite, bool hasBg)
    {
        if (floatingIcons == null) return;
        
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

        if (onValidateCauldronClick != null && !onValidateCauldronClick.Invoke()) return; 
    
        if (currentIngredients.Count == MaxIngredients)
            FinishPotion(false);
        else
            ShowErrorMessage($"Adicione {MaxIngredients} ingredientes primeiro");
    }

    public void BrewWithMagic()
    {
        if (sessionData != null && sessionData.potionAwaitingDelivery)
        {
            ShowErrorMessage("Leve primeiro a poção ao balcão");
            return;
        }

        if (currentIngredients.Count == MaxIngredients)
        {
            AudioManager.Instance.PlaySFX(spellCastedSound);
            if (spellFilter) spellFilter.SetActive(true);

            FinishPotion(true);
        }
        else
        {
            ShowErrorMessage($"Adicione {MaxIngredients} ingredientes primeiro");
        }
    }

    private void FinishPotion(bool isSpellUsed)
    {
        AudioManager.Instance.PlaySFX(potionPreparedSound);

        if (sessionData != null)
            sessionData.potionAwaitingDelivery = true;

        PotionDataSO visualToken = isSpellUsed ? magicPotionVisual : normalPotionVisual;
        BrewedPotion potion = new BrewedPotion(currentIngredients, isSpellUsed, visualToken);

        if (inventoryData && inventoryData.HasSpaceForPotion())
        {
            inventoryData.AddPotion(potion);

            foreach (ItemDataSO item in currentIngredients)
            {
                if (inventoryData.savedInv.ContainsKey(item))
                    inventoryData.savedInv[item] -= 1;
            }
            
            if (ingredientManager) ingredientManager.UpdateShelves();
        }

        currentIngredients.Clear();
        ResetVisuals();
        
        if (cauldronAnimator)
        {
            cauldronAnimator.SetBool(PotionReadyHash, true);
            cauldronAnimator.SetBool(SpellUsedHash, isSpellUsed);
        }

        if (popup) popup.ShowMessage("Poção enviada para o inventário");

        onPotionFinished?.Invoke(isSpellUsed);
    }

    private void ResetVisuals()
    {
        if (floatingIcons != null)
            foreach (Image icon in floatingIcons)
            {
                icon.enabled = false;
                icon.sprite = null;
            }

        if (cauldronAnimator != null)
            cauldronAnimator.SetBool("PotionStarted", false);
    }

    private void ShowErrorMessage(string popupText)
    {
        AudioManager.Instance.PlaySFX(errorSound);
        if (popup) popup.ShowMessage(popupText);
    }
}