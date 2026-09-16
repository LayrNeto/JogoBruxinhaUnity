using System;
using System.Collections.Generic;
using JogoBruxinha.Gameplay.Counter;
using JogoBruxinha.Gameplay.Inventory;
using UnityEngine;

namespace JogoBruxinha.Gameplay.Tutorial
{
    [Serializable]
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
    public sealed class TutorialDataSO : ScriptableObject
    {
        public int tutorialStep = 0;

        [Header("Agata Base Visuals")]
        public Sprite agataPovSprite;
        public Sprite agataDialogueBox;

        [Header("Quest Requirements")]
        [Tooltip("Quais plantas o jogador deve ter no inventário para passar do Step 1")]
        public List<PlantDataSO> requiredIngredients = new List<PlantDataSO>();
        public List<PlantDataSO> correctRecipe = new List<PlantDataSO>();

        [Header("Dialogues")]
        [Tooltip("Todos os diálogos do tutorial, identificados por ID.")]
        public List<TutorialDialogueBlock> allDialogues = new List<TutorialDialogueBlock>();

        public DialogueSequence GetDialogue(TutorialContext id)
        {
            if (allDialogues != null)
            {
                foreach (TutorialDialogueBlock block in allDialogues)
                {
                    if (block.interactionID == id)
                    {
                        return block.sequence;
                    }
                }
            }

            if (id != TutorialContext.DEFAULT)
            {
                Debug.LogWarning($"Diálogo com ID '{id}' não encontrado no TutorialDataSO!", this);
            }

            return new DialogueSequence { lines = new List<DialogueLine>() };
        }

        public void ResetTutorial()
        {
            tutorialStep = 0;
        }
    }
}