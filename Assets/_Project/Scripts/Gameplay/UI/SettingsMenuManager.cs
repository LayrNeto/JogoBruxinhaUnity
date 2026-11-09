using JogoBruxinha.Core.Audio;
using JogoBruxinha.Gameplay.GameFlow;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace JogoBruxinha.Gameplay.UI
{
    public sealed class SettingsMenuManager : MonoBehaviour
    {
        private const string MUSIC_KEY = "vol_music";
        private const string SFX_KEY = "vol_sfx";
        private const string VOICES_KEY = "vol_voices";

        [Header("References")]
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _sfxSlider;
        [SerializeField] private Slider _voicesSlider;
        [SerializeField] private Button _exitButton;

        private void Awake()
        {
            ValidateDependencies();
            ConfigureSliderRanges();
        }

        private void ValidateDependencies()
        {
            Debug.Assert(_musicSlider != null, $"Missing _musicSlider on {name}", this);
            Debug.Assert(_sfxSlider != null, $"Missing _sfxSlider on {name}", this);
            Debug.Assert(_voicesSlider != null, $"Missing _voicesSlider on {name}", this);
            Debug.Assert(_exitButton != null, $"Missing _exitButton on {name}", this);
        }

        private void ConfigureSliderRanges()
        {
            _musicSlider.minValue = 0.0001f;
            _musicSlider.maxValue = 1f;

            _sfxSlider.minValue = 0.0001f;
            _sfxSlider.maxValue = 1f;

            _voicesSlider.minValue = 0.0001f;
            _voicesSlider.maxValue = 1f;
        }

        private void OnEnable()
        {
            if (GameStateManager.Instance != null && GameStateManager.Instance.InputControls != null)
                GameStateManager.Instance.InputControls.UI.Close.performed += OnClosePerformed;

            _musicSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(MUSIC_KEY, 1f));
            _sfxSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(SFX_KEY, 1f));
            _voicesSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(VOICES_KEY, 1f));

            _musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
            _sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
            _voicesSlider.onValueChanged.AddListener(OnVoicesSliderChanged);
        }

        private void OnDisable()
        {
            if (GameStateManager.Instance != null && GameStateManager.Instance.InputControls != null)
                GameStateManager.Instance.InputControls.UI.Close.performed -= OnClosePerformed;

            _musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);
            _sfxSlider.onValueChanged.RemoveListener(OnSfxSliderChanged);
            _voicesSlider.onValueChanged.RemoveListener(OnVoicesSliderChanged);

            PlayerPrefs.Save();
        }

        private void OnClosePerformed(InputAction.CallbackContext context)
        {
            _exitButton.onClick.Invoke();
        }

        private void OnMusicSliderChanged(float val) => AudioManager.Instance.SetMusicVolume(val);
        private void OnSfxSliderChanged(float val) => AudioManager.Instance.SetSFXVolume(val);
        private void OnVoicesSliderChanged(float val) => AudioManager.Instance.SetVoicesVolume(val);
    }
}