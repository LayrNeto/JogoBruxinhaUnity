using UnityEngine;

namespace JogoBruxinha.Core.Audio
{
    [CreateAssetMenu(fileName = "NewSoundData", menuName = "Scriptable Objects/Audio/Sound Data")]
    public sealed class SoundDataSO : ScriptableObject
    {
        [field: SerializeField] 
        public AudioClip Clip { get; private set; }

        [field: SerializeField, Range(0f, 1f)] 
        public float Volume { get; private set; } = 1f;

        [field: SerializeField, Range(0.1f, 3f)] 
        public float Pitch { get; private set; } = 1f;

        [field: Header("Dialogue voice only")]
        [field: Tooltip("Extra voice gain in dB. 0 keeps the original level; +1.58 is about 20% more amplitude.")]
        [field: SerializeField, Range(-24f, 12f)]
        public float VoiceGainDb { get; private set; }

        [field: Tooltip("Maximum duration of each voice blip in seconds, independent of pitch. Blips never overlap.")]
        [field: SerializeField, Range(0.03f, 0.15f)]
        public float VoiceBlipDuration { get; private set; } = 0.08f;
    }
}
