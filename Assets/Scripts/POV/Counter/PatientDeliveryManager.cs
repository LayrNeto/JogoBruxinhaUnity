using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PatientDeliveryManager : MonoBehaviour
{
    [Header("UI References")]
    public Button potionButton; 
    public Image potionImage;
    public Image npcRenderer;

    [Header("Dependencies")]
    public SessionDataSO sessionData;
    public InventoryDataSO inventoryData;
    public NPCBrain npcBrain;
    
    [Header("Delegates/Hooks")]
    public Action onPotionDelivery;

    [Header("Events")]
    public GameEvent nightFallEvent;

    [Header("Audio")]
    public SoundDataSO deliverPotionSound;
    public SoundDataSO healPatientSound;

    private PatientDataSO currentPatient;
    private bool lastRecipeWasCorrect;
    private bool isWaitingDelivery = false;

    private void Start()
    {
        GameStateManager.Instance.inputControls.UI.Close.performed += ctx => ClosePOV();
    }

    private void OnDestroy()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.inputControls.UI.Close.performed -= ctx => ClosePOV();
        }
    }
    
    public void StartPatientEncounter(PatientDataSO patient, List<DialogueLine> introDialogue)
    {
        currentPatient = patient;
        isWaitingDelivery = true;
        
        if (npcRenderer && currentPatient.povCursedSprite != null)
        {
            npcRenderer.sprite = currentPatient.povCursedSprite;
            npcRenderer.SetNativeSize();
            npcRenderer.gameObject.SetActive(true);
        }

        if (sessionData.potionAwaitingDelivery && inventoryData.craftedPotions.Count > 0)
        {
            Debug.Log("Entrou no balcão com poção pronta pra ser entregue");
            potionButton.gameObject.SetActive(true);
            potionImage.sprite = inventoryData.craftedPotions[0].potionData.tableSprite;
        }
        else
        {
            Debug.Log("Sem poção pronta para entregar no balcão");
            potionButton.gameObject.SetActive(false);
        }

        DialogueManager.Instance.PlayDialogue(patient.dialogueBoxSprite, introDialogue, () => {
            potionButton.enabled = true;
        });
    }

    public void DeliverPotion()
    {
        AudioManager.Instance.PlaySFX(deliverPotionSound);
        if (onPotionDelivery != null)
        {
            onPotionDelivery.Invoke();
            return;
        }

        potionButton.gameObject.SetActive(false);
        sessionData.potionAwaitingDelivery = false; 
        isWaitingDelivery = false;

        if (inventoryData.craftedPotions.Count == 0)
        {
            Debug.LogWarning("Tentou entregar, mas não tem poção no inventário!");
            return;
        }
        
        BrewedPotion craftedPotion = inventoryData.craftedPotions[0];
        bool isMagicPotion = craftedPotion.isSpellUsed; 

        lastRecipeWasCorrect = isMagicPotion || VerifyRecipe(); 
        if (lastRecipeWasCorrect && npcRenderer)
        {
            npcRenderer.sprite = currentPatient.povHealedSprite;
            npcRenderer.SetNativeSize();
            AudioManager.Instance.PlaySFX(healPatientSound);
        }

        inventoryData.craftedPotions.Clear();
        inventoryData.savedInv[craftedPotion.potionData]--;

        List<DialogueLine> rawDialogue = lastRecipeWasCorrect ? currentPatient.successDialogue : currentPatient.failDialogue;
        List<DialogueLine> filteredDialogue = FilterMagicDialogue(rawDialogue, isMagicPotion);

        DialogueManager.Instance.PlayDialogue(currentPatient.dialogueBoxSprite, filteredDialogue);

        sessionData.isNight = true;
        nightFallEvent?.Raise();
    }

    
    public void ClosePOV()
    {
        if (!isWaitingDelivery && currentPatient != null && npcBrain != null)
        {
            npcBrain.LeaveShop(lastRecipeWasCorrect);
        }

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.ForceCloseDialogue();
        }

        gameObject.SetActive(false);
        
    }

    private bool VerifyRecipe()
    {
        List<PlantDataSO> potionIngredients = inventoryData.craftedPotions[0].ingredients;
        List<PlantDataSO> patientRecipe = currentPatient.requiredPlants;

        HashSet<PlantDataSO> potionSet = new HashSet<PlantDataSO>(potionIngredients);
        HashSet<PlantDataSO> recipeSet = new HashSet<PlantDataSO>(patientRecipe);

        return potionSet.SetEquals(recipeSet);
    }

    private List<DialogueLine> FilterMagicDialogue(List<DialogueLine> originalList, bool potionHasMagic)
    {
        List<DialogueLine> filteredList = new List<DialogueLine>();

        foreach (DialogueLine line in originalList)
        {
            if (line.requiresMagic && !potionHasMagic)
            {
                continue;
            }
            filteredList.Add(line);
        }

        return filteredList;
    }
}