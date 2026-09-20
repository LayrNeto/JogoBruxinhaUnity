using System.Collections;
using JogoBruxinha.Core.Analytics;
using JogoBruxinha.Core.SceneManagement;
using JogoBruxinha.Gameplay.GameFlow;
using JogoBruxinha.Gameplay.SaveSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JogoBruxinha.Gameplay.UI
{
    public sealed class PauseManager : MonoBehaviour
    {
        [Header("UI Reference")]
        [SerializeField] private GameObject _pauseCanvas;
        [SerializeField] private GameObject _savingScreen;

        private Coroutine _saveCoroutine;

        private void Awake()
        {
            Debug.Assert(_pauseCanvas != null, $"Missing _pauseCanvas reference on {name}", this);
            Debug.Assert(_savingScreen != null, $"Missing _savingScreen reference on {name}", this);
        }

        private void OnEnable()
        {
            if (GameStateManager.Instance != null && GameStateManager.Instance.InputControls != null)
            {
                GameStateManager.Instance.InputControls.Player.Pause.performed += OnPausePerformed;
                GameStateManager.Instance.InputControls.UI.Close.performed += OnClosePerformed;
            }
        }

        private void OnDisable()
        {
            if (GameStateManager.Instance != null && GameStateManager.Instance.InputControls != null)
            {
                GameStateManager.Instance.InputControls.Player.Pause.performed -= OnPausePerformed;
                GameStateManager.Instance.InputControls.UI.Close.performed -= OnClosePerformed;
            }

            if (_saveCoroutine != null)
            {
                StopCoroutine(_saveCoroutine);
                _saveCoroutine = null;
            }
        }

        private void OnPausePerformed(InputAction.CallbackContext ctx)
        {
            Pause();
        }

        private void OnClosePerformed(InputAction.CallbackContext ctx)
        {
            Resume();
        }

        public void Resume()
        {
            if (_pauseCanvas == null || !_pauseCanvas.activeSelf) return;

            _pauseCanvas.SetActive(false);

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.PopState();
            }
        }

        public void Quit()
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        public void Menu()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.ResetToDefaultState();
            }

            // Playtest Analytics ==================================
            if (PlaytestLogger.Instance != null)
            {
                PlaytestLogger.Instance.EndAndSaveSession();
            }
            // Playtest Analytics ==================================

            if (FadeManager.Instance != null)
            {
                FadeManager.Instance.StartTransition("MainMenu", string.Empty, 0.2f, 0.2f);
            }
        }

        public void Pause()
        {
            if (_pauseCanvas == null) return;

            _pauseCanvas.SetActive(true);

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.PushState(GameStateManager.GameState.PAUSE);
            }
        }

        public void Save()
        {
            if (_saveCoroutine != null)
            {
                StopCoroutine(_saveCoroutine);
            }

            _saveCoroutine = StartCoroutine(SaveRoutine());
        }

        private IEnumerator SaveRoutine()
        {
            if (_savingScreen != null)
            {
                _savingScreen.SetActive(true);
            }

            yield return new WaitForSecondsRealtime(2.4f);

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SaveGame();
            }

            if (_savingScreen != null)
            {
                _savingScreen.SetActive(false);
            }

            _saveCoroutine = null;
        }
    }
}