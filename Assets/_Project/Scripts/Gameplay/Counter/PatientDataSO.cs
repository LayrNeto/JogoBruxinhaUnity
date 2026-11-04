using System.Collections.Generic;
using JogoBruxinha.Core.Audio;
using JogoBruxinha.Gameplay.Inventory;
using UnityEngine;
using UnityEngine.Localization;

namespace JogoBruxinha.Gameplay.Counter
{
    public enum SpeakerType
    {
        NPC,
        Player
    }

    [System.Serializable]
    public struct DialogueLine
    {
        public SpeakerType speaker;

        public LocalizedString text;
        public bool requiresMagic;

        public string GetLocalizedText()
        {
            if (text == null || text.IsEmpty)
                return string.Empty;

            return text.GetLocalizedString();
        }
    }

    [System.Serializable]
    public struct DialogueSequence
    {
        public List<DialogueLine> lines;
    }

    [CreateAssetMenu(fileName = "NewPatientData", menuName = "Scriptable Objects/Game Data/Patient Data")]
    public sealed class PatientDataSO : ScriptableObject
    {
        [Header("Identification")]
        public string patientId;
        public LocalizedString patientName;

        [Header("Top-Down Data")]
        public float cursedMovementSpeed = 1f;
        public float healedMovementSpeed = 1f;
        public RuntimeAnimatorController animatorController;

        [Header("Visuals")]
        public Sprite povCursedSprite;
        public Sprite povHealedSprite;
        public Sprite dialogueBoxSprite;

        [Header("Dialogue Audio")]
        public SoundDataSO voiceSound;

        [Header("Recipe Scoring Requirements")]
        public int targetScore = 30;
        public List<SymptomScoreWeight> symptomWeights = new List<SymptomScoreWeight>();

        [Header("Dialogue: First Meeting")]
        public List<DialogueLine> introDialogue = new List<DialogueLine>();

        [Header("Dialogue: Waiting for Potion")]
        public List<DialogueSequence> waitingDialogues = new List<DialogueSequence>();

        [Header("Dialogue: Delivery Success")]
        public List<DialogueLine> successDialogue = new List<DialogueLine>();

        [Header("Dialogue: Delivery Failed")]
        public List<DialogueLine> failDialogue = new List<DialogueLine>();


        public int EvaluatePotionScore(HashSet<SymptomType> potionSymptoms)
        {
            if (potionSymptoms == null || symptomWeights == null) return 0;

            int totalScore = 0;

            foreach (var weight in symptomWeights)
            {
                if (potionSymptoms.Contains(weight.symptom))
                {
                    totalScore += weight.points;
                }
            }            

            return totalScore;
        }

        public string GetLocalizedPatientName()
        {
            if (patientName == null || patientName.IsEmpty)
                return patientId;

            return patientName.GetLocalizedString();
        }
    }
}
