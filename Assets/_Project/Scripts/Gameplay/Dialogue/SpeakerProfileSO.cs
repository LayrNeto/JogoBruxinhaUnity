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

        [Header("Voice Settings")]
        public SoundDataSO voiceSound;
        [Tooltip("Extra voice gain in dB. 0 keeps the original level; +1.58 is about 20% more amplitude.")]
        [Range(-24f, 12f)] public float voiceGainDb = 0f;
        [Tooltip("Maximum duration of each voice blip in seconds. Blips never overlap.")]
        [Range(0.03f, 0.15f)] public float voiceBlipDuration = 0.08f;

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
