using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace JogoBruxinha.Core.Audio
{
    public sealed class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer _mainMixer;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioSource _voiceSource;
        [SerializeField] private AudioSource _uiSource;

        public AudioSource VoiceSource => _voiceSource;

        private Coroutine fadeCoroutine;

        public void Init()
        {
            if (Instance == null)
            {
                Instance = this;
                ValidateDependencies();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void ValidateDependencies()
        {
            Debug.Assert(_mainMixer != null, $"Missing _mainMixer reference on {name}", this);
            Debug.Assert(_musicSource != null, $"Missing _musicSource reference on {name}", this);
            Debug.Assert(_sfxSource != null, $"Missing _sfxSource reference on {name}", this);
            Debug.Assert(_voiceSource != null, $"Missing _voiceSource reference on {name}", this);
            Debug.Assert(_uiSource != null, $"Missing _uiSource reference on {name}", this);
        }

        private IEnumerator Start()
        {
            yield return null;

            float music = PlayerPrefs.GetFloat("vol_music", 1f);
            float sfx = PlayerPrefs.GetFloat("vol_sfx", 1f);
            float voices = PlayerPrefs.GetFloat("vol_voices", 1f);

            SetVolume("MusicVolume", music);
            SetVolume("SFXVolume", sfx);
            SetVolume("VoicesVolume", voices);
        }

        private void SetVolume(string paramName, float linearValue)
        {
            if (_mainMixer == null) return;
            float dbValue = Mathf.Log10(Mathf.Clamp(linearValue, 0.0001f, 1f)) * 20f;
            _mainMixer.SetFloat(paramName, dbValue);
        }

        public void PlayMusic(SoundDataSO musicData)
        {
            if (musicData == null || musicData.Clip == null || _musicSource == null) return;

            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }

            if (_musicSource.clip == musicData.Clip && _musicSource.isPlaying) return;

            _musicSource.Stop();
            _musicSource.clip = musicData.Clip;
            _musicSource.volume = Mathf.Clamp01(musicData.Volume);
            _musicSource.pitch = musicData.Pitch;
            _musicSource.loop = true;
            _musicSource.Play();
        }

        public void StopMusic(float fadeDuration = 0f)
        {
            if (_musicSource == null) return;

            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }

            if (fadeDuration > 0f && gameObject.activeInHierarchy)
            {
                fadeCoroutine = StartCoroutine(FadeOutRoutine(fadeDuration));
            }
            else
            {
                _musicSource.Stop();
                _musicSource.clip = null;
            }
        }

        private IEnumerator FadeOutRoutine(float duration)
        {
            float startVolume = _musicSource.volume;
            float t = 0f;

            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                _musicSource.volume = Mathf.Lerp(startVolume, 0f, t / duration);
                yield return null;
            }

            _musicSource.Stop();
            _musicSource.clip = null;
            _musicSource.volume = startVolume;
            fadeCoroutine = null;
        }

        public void PlaySFX(SoundDataSO soundData)
        {
            if (soundData == null || soundData.Clip == null) return;

            if (_sfxSource == null) return;
            
            _sfxSource.pitch = soundData.Pitch;
            _sfxSource.PlayOneShot(soundData.Clip, soundData.Volume); 
        }

        public void PlayUI(SoundDataSO soundData)
        {
            if (soundData == null || soundData.Clip == null) return;

            if (_uiSource == null) return;

            _uiSource.pitch = soundData.Pitch;
            _uiSource.PlayOneShot(soundData.Clip, soundData.Volume);
        }

        public void PlayVoiceBlip(SoundDataSO voiceData)
        {
            if (voiceData == null || voiceData.Clip == null) return;

            _voiceSource.Stop(); 
            _voiceSource.pitch = voiceData.Pitch;
            _voiceSource.volume = voiceData.Volume;
            _voiceSource.clip = voiceData.Clip;
            _voiceSource.Play();
        }

        public void SetMusicVolume(float linearValue)
        {
            ApplyVolume("MusicVolume", linearValue);
            PlayerPrefs.SetFloat("vol_music", linearValue);
        }

        public void SetSFXVolume(float linearValue)
        {
            ApplyVolume("SFXVolume", linearValue);
            PlayerPrefs.SetFloat("vol_sfx", linearValue);
        }

        public void SetVoicesVolume(float linearValue)
        {
            ApplyVolume("VoicesVolume", linearValue);
            PlayerPrefs.SetFloat("vol_voices", linearValue);
        }

        private void ApplyVolume(string paramName, float linearValue)
        {
            if (_mainMixer == null) return;
            float dbValue = Mathf.Log10(Mathf.Clamp(linearValue, 0.0001f, 1f)) * 20f;
            _mainMixer.SetFloat(paramName, dbValue);
        }    
    }
}