using System.Collections;
using JogoBruxinha.Core.Analytics;
using JogoBruxinha.Core.Audio;
using JogoBruxinha.Core.SceneManagement;
using JogoBruxinha.Gameplay.GameFlow;
using JogoBruxinha.Gameplay.Inventory;
using JogoBruxinha.Gameplay.SaveSystem;
using JogoBruxinha.Gameplay.Tutorial;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace JogoBruxinha.Gameplay.UI
{
    public sealed class MainMenuManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _tutorialCanvas;
        [SerializeField] private Button _continueButton;

        [Header("Data")]
        [SerializeField] private SessionDataSO _sessionData;
        [SerializeField] private TutorialDataSO _tutorialData;
        [SerializeField] private InventoryDataSO _inventoryData;

        [Header("Audio")]
        [SerializeField] private SoundDataSO _mainMenuMusic;

        private Coroutine _continueCoroutine;

        private void Awake()
        {
            Debug.Assert(_tutorialCanvas != null, $"Missing _tutorialCanvas reference on {name}", this);
            Debug.Assert(_continueButton != null, $"Missing _continueButton reference on {name}", this);
            Debug.Assert(_sessionData != null, $"Missing _sessionData reference on {name}", this);
            Debug.Assert(_tutorialData != null, $"Missing _tutorialData reference on {name}", this);
            Debug.Assert(_inventoryData != null, $"Missing _inventoryData reference on {name}", this);
        }

        private void Start()
        {
            if (_continueButton != null)
            {
                _continueButton.interactable = SaveManager.Instance != null && SaveManager.Instance.HasSaveFile() && PlaytestLogger.Instance == null;
            }

            if (_mainMenuMusic != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMusic(_mainMenuMusic);
            }
        }

        private void OnEnable()
        {
            if (GameStateManager.Instance != null && GameStateManager.Instance.InputControls != null)
            {
                GameStateManager.Instance.InputControls.UI.Close.performed += OnCloseTutorialCanvas;
            }
        }

        private void OnDisable()
        {
            if (GameStateManager.Instance != null && GameStateManager.Instance.InputControls != null)
            {
                GameStateManager.Instance.InputControls.UI.Close.performed -= OnCloseTutorialCanvas;
            }

            if (_continueCoroutine != null)
            {
                StopCoroutine(_continueCoroutine);
                _continueCoroutine = null;
            }
        }

        private void OnCloseTutorialCanvas(InputAction.CallbackContext ctx)
        {
            CloseTutorialCanvas();
        }

        private void CloseTutorialCanvas()
        {
            if (_tutorialCanvas != null)
            {
                _tutorialCanvas.SetActive(false);
            }
        }

        public void StartTutorial()
        {
            _tutorialData.ResetTutorial();
            _sessionData.ResetSession();
            _inventoryData.ClearData();

            _sessionData.tutorialData = _tutorialData;

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.PopState();
                GameStateManager.Instance.PushState(GameStateManager.GameState.TOP_DOWN);
            }

            if (FadeManager.Instance != null)
            {
                FadeManager.Instance.StartTransition("HouseScene", "TutorialSpawn", 1f, 1f);

                // Playtest Analytics ==================================
                PlaytestLogger.Instance.StartNewSession();
                // Playtest Analytics ==================================
            }
        }

        public void SkipTutorial()
        {
            _tutorialData.ResetTutorial();
            _sessionData.ResetSession();
            _inventoryData.ClearData();

            _sessionData.tutorialData = null;

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.PopState();
                GameStateManager.Instance.PushState(GameStateManager.GameState.TOP_DOWN);
            }

            if (FadeManager.Instance != null)
            {
                FadeManager.Instance.StartTransition("HouseScene", "BedSpawn", 1f, 1f);

                // Playtest Analytics ==================================
                PlaytestLogger.Instance.StartNewSession();
                // Playtest Analytics ==================================
            }
        }

        public void Quit()
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        public void ContinueGame()
        {
            if (SaveManager.Instance == null) return;

            SaveManager.Instance.LoadGame();

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.PopState();
                GameStateManager.Instance.PushState(GameStateManager.GameState.TOP_DOWN);
            }

            if (_continueCoroutine != null)
            {
                StopCoroutine(_continueCoroutine);
            }

            _continueCoroutine = StartCoroutine(ContinueGameRoutine());
        }

        private IEnumerator ContinueGameRoutine()
        {
            yield return new WaitForSeconds(2.5f);

            string savedScene = _sessionData != null ? _sessionData.currentScene : "HouseScene";

            if (FadeManager.Instance != null)
            {
                FadeManager.Instance.StartTransition(savedScene, "SAVED_SPAWN", 1f, 1f);
            }

            _continueCoroutine = null;
        }
    }
}