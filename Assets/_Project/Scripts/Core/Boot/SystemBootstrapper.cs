using JogoBruxinha.Core.Audio;
using JogoBruxinha.Core.SceneManagement;
using UnityEngine;

namespace JogoBruxinha.Core.Boot
{
    [DefaultExecutionOrder(-100)]
    public sealed class SystemBootstrapper : MonoBehaviour
    {
        [Header("Singletons")]
        [SerializeField] private FadeManager _fadeManager;
        [SerializeField] private AudioManager _audioManager;

        private void Awake()
        {
            Debug.Assert(_fadeManager != null, $"Missing FadeManager reference on {name}", this);
            Debug.Assert(_audioManager != null, $"Missing AudioManager reference on {name}", this);

            _fadeManager.Init();
            _audioManager.Init();
        }
    }
}