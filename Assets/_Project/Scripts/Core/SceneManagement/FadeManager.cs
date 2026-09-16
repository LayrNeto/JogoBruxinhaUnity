using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JogoBruxinha.Core.SceneManagement
{
    public sealed class FadeManager : MonoBehaviour
    {
        public static FadeManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private Image blackScreen;

        public bool IsFading { get; private set; }
        public string TargetSpawnID { get; private set; }

        public void Init()
        {
            if (Instance == null)
            {
                Instance = this;
                Debug.Assert(blackScreen != null, $"Missing blackScreen reference on {name}", this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // ==========================================
        // Public API
        // ==========================================

        public void StartTransition(string sceneName, string spawnID, float fadeOutTime, float fadeInTime, Action onFadeOutComplete = null)
        {
            if (IsFading || blackScreen == null) return;

            StartCoroutine(FadeRoutine(sceneName, spawnID, fadeOutTime, fadeInTime, onFadeOutComplete));
        }

        public void StartFadeOut(float time, Action onFadeOutComplete = null)
        {
            if (IsFading || blackScreen == null) return;

            StartCoroutine(SingleFadeRoutine(0f, 1f, time, onFadeOutComplete));
        }

        public void StartFadeIn(float time, Action onFadeInComplete = null)
        {
            if (IsFading || blackScreen == null) return;

            StartCoroutine(SingleFadeRoutine(1f, 0f, time, onFadeInComplete));
        }

        public void StartFullFade(float fadeOutTime, float fadeInTime, Action onFadeOutComplete = null)
        {
            if (IsFading || blackScreen == null) return;

            StartCoroutine(FullFadeRoutine(fadeOutTime, fadeInTime, onFadeOutComplete));
        }

        // ==========================================
        // Coroutines 
        // ==========================================

        private IEnumerator FadeRoutine(string sceneName, string spawnID, float fadeOutTime, float fadeInTime, Action onFadeOutComplete)
        {
            IsFading = true;
            TargetSpawnID = spawnID;

            Color c = blackScreen.color;
            blackScreen.raycastTarget = true;

            float startAlpha = blackScreen.color.a;
            float t = 0f;

            while (t < fadeOutTime)
            {
                t += Time.deltaTime;
                c.a = Mathf.Lerp(startAlpha, 1f, t / fadeOutTime);
                blackScreen.color = c;
                yield return null;
            }

            onFadeOutComplete?.Invoke();

            SceneManager.LoadScene(sceneName);
            yield return null;

            t = 0f;
            while (t < fadeInTime)
            {
                t += Time.unscaledDeltaTime;
                c.a = Mathf.Lerp(1f, 0f, t / fadeInTime);
                blackScreen.color = c;
                yield return null;
            }

            blackScreen.raycastTarget = false;
            IsFading = false;
        }

        private IEnumerator SingleFadeRoutine(float startAlpha, float targetAlpha, float duration, Action onFadeComplete)
        {
            IsFading = true;
            Color c = blackScreen.color;
            blackScreen.raycastTarget = targetAlpha > 0f;

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                c.a = Mathf.Lerp(startAlpha, targetAlpha, t / duration);
                blackScreen.color = c;
                yield return null;
            }

            onFadeComplete?.Invoke();
            IsFading = false;
        }

        private IEnumerator FullFadeRoutine(float fadeOutTime, float fadeInTime, Action onFadeOutComplete)
        {
            IsFading = true;
            blackScreen.raycastTarget = true;
            Color c = blackScreen.color;

            float t = 0f;
            while (t < fadeOutTime)
            {
                t += Time.deltaTime;
                c.a = Mathf.Lerp(0f, 1f, t / fadeOutTime);
                blackScreen.color = c;
                yield return null;
            }

            onFadeOutComplete?.Invoke();

            t = 0f;
            while (t < fadeInTime)
            {
                t += Time.unscaledDeltaTime;
                c.a = Mathf.Lerp(1f, 0f, t / fadeInTime);
                blackScreen.color = c;
                yield return null;
            }

            blackScreen.raycastTarget = false;
            IsFading = false;
        }
    }
}