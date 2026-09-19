using System.Collections.Generic;
using JogoBruxinha.Core.Audio;
using JogoBruxinha.Gameplay.Inventory;
using UnityEngine;

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

        [TextArea(3, 5)]
        public string text;
        public bool requiresMagic;
    }

    [System.Serializable]
    public struct DialogueSequence
    {
        public List<DialogueLine> lines;
    }

    [CreateAssetMenu(fileName = "NewPatientData", menuName = "Scriptable Objects/Game Data/Patient Data")]
    public sealed class PatientDataSO : ScriptableObject
    {
        [Header("Top-Down Data")]
        public float cursedMovementSpeed = 1f;
        public float healedMovementSpeed = 1f;
        public RuntimeAnimatorController animatorController;

        [Header("Visuals")]
        public string clientName;
        public Sprite povCursedSprite;
        public Sprite povHealedSprite;
        public Sprite dialogueBoxSprite;

        [Header("Dialogue Audio")]
        public SoundDataSO voiceSound;

        [Header("Recipe Requirements")]
        public List<PlantDataSO> requiredPlants = new List<PlantDataSO>();

        [Header("Dialogue: First Meeting")]
        public List<DialogueLine> introDialogue = new List<DialogueLine>();

        [Header("Dialogue: Waiting for Potion")]
        public List<DialogueSequence> waitingDialogues = new List<DialogueSequence>();

        [Header("Dialogue: Delivery Success")]
        public List<DialogueLine> successDialogue = new List<DialogueLine>();

        [Header("Dialogue: Delivery Failed")]
        public List<DialogueLine> failDialogue = new List<DialogueLine>();
    }
}
