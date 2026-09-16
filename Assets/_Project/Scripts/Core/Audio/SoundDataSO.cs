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
    }
}