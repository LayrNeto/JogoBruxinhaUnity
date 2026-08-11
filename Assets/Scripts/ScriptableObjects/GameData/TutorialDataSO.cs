using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public struct TutorialDialogueBlock
{
    public TutorialContext interactionID; 
    public DialogueSequence sequence;
}

public enum TutorialContext
{
    DEFAULT,
    BALCAO_STEP_0,
    BALCAO_STEP_1,
    CALDEIRAO_INICIO_STEP_2,
    CALDEIRAO_FIM_STEP_2,
    BALCAO_STEP_3,
    CALDEIRAO_INICIO_STEP_4,
    CALDEIRAO_PRE_POCAO_STEP_4,
    CALDEIRAO_FIM_STEP_4,
    POS_CUTSCENE_STEP_5,
    CALDEIRAO_ERRO_RECEITA,
    CALDEIRAO_ERRO_CLIQUE,
}


[CreateAssetMenu(fileName = "NewTutorialData", menuName = "Scriptable Objects/Game Data/Tutorial Data")]
public class TutorialDataSO : ScriptableObject
{
    public int tutorialStep = 0;

    [Header("Agata Base Visuals")]
    public Sprite agataPovSprite;
    public Sprite agataDialogueBox;

    [Header("Quest Requirements")]
    [Tooltip("Quais plantas o jogador deve ter no inventário para passar do Step 1")]
    public List<PlantDataSO> requiredIngredients;
    public List<PlantDataSO> correctRecipe;

    [Header("Dialogues")]
    [Tooltip("Todos os diálogos do tutorial, identificados por ID.")]
    public List<TutorialDialogueBlock> allDialogues;

    public DialogueSequence GetDialogue(TutorialContext id)
    {
        foreach (var block in allDialogues)
        {
            if (block.interactionID == id)
                return block.sequence;
        }
        
        if (id != TutorialContext.DEFAULT) Debug.LogWarning($"Diálogo com ID '{id}' não encontrado no TutorialDataSO!");
        return new DialogueSequence();
    }

    public void ResetTutorial()
    {
        tutorialStep = 0;
    }
}