using JogoBruxinha.Core.Audio;
using UnityEngine;
using UnityEngine.Localization;

namespace JogoBruxinha.Gameplay.Dialogue
{
    [CreateAssetMenu(fileName = "Speaker_", menuName = "Scriptable Objects/Dialogue/Speaker Profile")]
    public sealed class SpeakerProfileSO : ScriptableObject
    {
        [Header("Identification")]
        public LocalizedString speakerName;
        public Color nameColor = Color.white;

        [Header("Audio")]
        public SoundDataSO voiceSound;

        [Header("Optional Dialogue Visuals")]
        public Sprite customDialogueBox;
        public Sprite customDialogueTitle;

        public string GetLocalizedName()
        {
            if (speakerName == null || speakerName.IsEmpty)
                return string.Empty;
            return speakerName.GetLocalizedString();
        }
    }
}
