using System.Collections;
using UnityEngine;

namespace JogoBruxinha.Core.Audio
{
    public sealed class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _uiSource;

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
            Debug.Assert(_musicSource != null, $"Missing _musicSource reference on {name}", this);
            Debug.Assert(_uiSource != null, $"Missing _uiSource reference on {name}", this);
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
            if (soundData == null || soundData.Clip == null)
            {
                Debug.LogWarning("Null Reference playing SFX", this);
                return;
            }

            if (_uiSource == null) return;

            _uiSource.pitch = soundData.Pitch;
            _uiSource.PlayOneShot(soundData.Clip, soundData.Volume);
        }
    }
}