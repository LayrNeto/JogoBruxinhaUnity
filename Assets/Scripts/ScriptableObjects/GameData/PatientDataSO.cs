using System.Collections.Generic;
using UnityEngine;

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
public class PatientDataSO : ScriptableObject
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

    [Header("Recipe Requirements")]
    public List<PlantDataSO> requiredPlants;

    [Header("Dialogue: First Meeting")]
    public List<DialogueLine> introDialogue; 

    [Header("Dialogue: Waiting for Potion")]
   public List<DialogueSequence> waitingDialogues;

    [Header("Dialogue: Delivery Success")]
    public List<DialogueLine> successDialogue;

    [Header("Dialogue: Delivery Failed")]
    public List<DialogueLine> failDialogue;

}