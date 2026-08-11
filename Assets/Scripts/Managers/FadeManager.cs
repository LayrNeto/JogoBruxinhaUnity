using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance { get; private set; }

    [Header("Configs")]
    public Image blackScreen;

    public bool isFading { get; private set; }
    public string targetSpawnID { get; private set; }

    public void Init()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); 
            return;
        }
    }

    public void StartTransition(string sceneName, string spawnID, float fadeOutTime, float fadeInTime, Action onFadeOutComplete = null)
    {
        if (isFading) return;

        StartCoroutine(FadeRoutine(sceneName, spawnID, fadeOutTime, fadeInTime, onFadeOutComplete));
    }

    private IEnumerator FadeRoutine(string sceneName, string spawnID, float fadeOutTime, float fadeInTime, Action onFadeOutComplete)
    {
        isFading = true;
        targetSpawnID = spawnID;

        blackScreen.raycastTarget = true;

        float startAlpha = blackScreen.color.a;
        
        float t = 0;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            blackScreen.color = new Color(0, 0, 0, Mathf.Lerp(startAlpha, 1, t / fadeOutTime));
            yield return null;
        }

        onFadeOutComplete?.Invoke();

        Debug.Log($"Carregando a cena {sceneName} com o spawnpoint {spawnID}");
        SceneManager.LoadScene(sceneName);
        yield return null;

        t = 0;
        while (t < fadeInTime)
        {
            t += Time.unscaledDeltaTime;
            blackScreen.color = new Color(0, 0, 0, Mathf.Lerp(1, 0, t / fadeInTime));
            yield return null;
        }

        blackScreen.raycastTarget = false;
        isFading = false;
    }

    public void StartFadeOut(float time, Action onFadeOutComplete = null)
    {
        if (!isFading) StartCoroutine(SingleFadeRoutine(0, 1, time, onFadeOutComplete));
    }

    public void StartFadeIn(float time, Action onFadeInComplete = null)
    {
        if (!isFading) StartCoroutine(SingleFadeRoutine(1, 0, time, onFadeInComplete));
    }

    private IEnumerator SingleFadeRoutine(float startAlpha, float targetAlpha, float duration, Action onFadeComplete)
    {
        isFading = true;
        blackScreen.raycastTarget = targetAlpha > 0;
        
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            blackScreen.color = new Color(0, 0, 0, Mathf.Lerp(startAlpha, targetAlpha, t / duration));
            yield return null;
        }

        onFadeComplete?.Invoke();
        
        isFading = false;
    }

    public void StartFullFade(float fadeOutTime, float fadeInTime, Action onFadeOutComplete = null)
    {
        if (!isFading) StartCoroutine(FullFadeRoutine(fadeOutTime, fadeInTime, onFadeOutComplete));
    }

    private IEnumerator FullFadeRoutine(float fadeOutTime, float fadeInTime, Action onFadeOutComplete)
    {
        isFading = true;

        blackScreen.raycastTarget = true;
        
        float t = 0;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            blackScreen.color = new Color(0, 0, 0, Mathf.Lerp(0, 1, t / fadeOutTime));
            yield return null;
        }

        onFadeOutComplete?.Invoke();

        t = 0;
        while (t < fadeInTime)
        {
            t += Time.unscaledDeltaTime;
            blackScreen.color = new Color(0, 0, 0, Mathf.Lerp(1, 0, t / fadeInTime));
            yield return null;
        }

        blackScreen.raycastTarget = false;
        isFading = false;
    }
}