using UnityEngine;

namespace JogoBruxinha.Core.Audio
{
    public sealed class SceneMusic : MonoBehaviour
    {
        [Header("Scene Music")]
        [SerializeField] private SoundDataSO _musicData;

        private void Awake()
        {
            Debug.Assert(_musicData != null, $"Missing a SoundDataSO on {name}", this);
        }

        private void Start()
        {
            if (_musicData != null && AudioManager.Instance != null)
                AudioManager.Instance.PlayMusic(_musicData);
        }
    }
}