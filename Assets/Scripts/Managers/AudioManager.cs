using System.Collections;
using UnityEngine;

public sealed class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource uiSource;

    public void Init()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlayMusic(SoundDataSO musicData)
    {
        if (musicData == null || musicData.clip == null) return;

        Debug.Log($"Playing {musicData} music");

        if (musicSource.clip == musicData.clip && musicSource.isPlaying) return;

        musicSource.Stop();

        musicSource.clip = musicData.clip;
        musicSource.volume = Mathf.Clamp01(musicData.volume); 
        musicSource.pitch = musicData.pitch;                 
        musicSource.loop = true;
        
        musicSource.Play();
    }

    public void StopMusic(float fadeDuration = 0f)
    {
        if (fadeDuration > 0f)
        {
            StartCoroutine(FadeOutRoutine(fadeDuration));
        }
        else
        {
            musicSource.Stop();
            musicSource.clip = null;
        }
    }

    private IEnumerator FadeOutRoutine(float duration)
    {
        float startVolume = musicSource.volume;
        float t = 0;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = null;
        musicSource.volume = startVolume;
    }

    public void PlaySFX(SoundDataSO soundData)
    {
        if (soundData == null || soundData.clip == null){
            Debug.LogWarning("Null Reference playing SFX");
            return;
        }
        
        uiSource.PlayOneShot(soundData.clip, soundData.volume); 
    }
}