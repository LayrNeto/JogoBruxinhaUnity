using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using JogoBruxinha.Core.SceneManagement;
using JogoBruxinha.Gameplay.Cauldron;
using JogoBruxinha.Gameplay.Characters.Player;
using JogoBruxinha.Gameplay.Counter;
using JogoBruxinha.Gameplay.Dialogue;
using JogoBruxinha.Gameplay.GameFlow;
using JogoBruxinha.Gameplay.Inventory;
using JogoBruxinha.Core.Audio;
using JogoBruxinha.Core.Analytics;

namespace JogoBruxinha.Gameplay.Tutorial
{
    public sealed class TutorialManager : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private SessionDataSO _sessionData;
        [SerializeField] private InventoryDataSO _inventoryData;

        [Header("UI References")]
        [SerializeField] private Image _npcImage;

        [Header("Cauldron References")]
        [SerializeField] private CauldronController _cauldronController;
        [SerializeField] private Button _spellButton;
        [SerializeField] private Button _exitButton;

        [Header("Counter References")]
        [SerializeField] private PatientDeliveryManager _patientDelivery;

        [Header("Cutscene References (Step 5)")]
        [SerializeField] private Transform _agataTopDownTransform;
        [SerializeField] private SpriteRenderer _agataTopDownSpriteRenderer;
        [SerializeField] private Sprite _agataCatSprite;
        [SerializeField] private GameObject _smokePrefab;
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private CinemachineCamera _vcam;

        [Header("Tutorial Logic")]
        [Tooltip("Cada índice do array representa um Step do tutorial (0, 1, 2...)")]
        [SerializeField] private UnityEvent[] _tutorialSteps = new UnityEvent[0];

        private Transform _originalCameraTarget;
        private Coroutine _transformationCoroutine;

        private void Awake()
        {
            Debug.Assert(_sessionData != null, $"Missing _sessionData reference on {name}", this);
            Debug.Assert(_inventoryData != null, $"Missing _inventoryData reference on {name}", this);
            Debug.Assert(_npcImage != null, $"Missing _npcImage reference on {name}", this);

            if (_sessionData == null || _sessionData.tutorialData == null) return;

            ApplyCurrentStep();
        }

        private void Start()
        {
            if (_sessionData != null && _sessionData.tutorialData != null)
            {
                EvaluateSceneLoadState();
            }
        }

        private void OnDisable()
        {
            if (_transformationCoroutine != null)
            {
                StopCoroutine(_transformationCoroutine);
                _transformationCoroutine = null;
            }
        }

        private void EvaluateSceneLoadState()
        {
            int step = _sessionData.tutorialData.tutorialStep;

            if (step == 1)
            {
                if (CollectedAllIngredients())
                {
                    Debug.Log("Jogador voltou da estufa com todos os ingredientes! Avançando para Step 2.");
                    AdvanceTutorial();
                }
                else
                {
                    Debug.Log("Jogador voltou da estufa, mas faltam ingredientes. Continua no Step 1.");
                }
            }
            else if (step == 5)
            {
            StartTransformationCutscene();
            }
        }

        private bool CollectedAllIngredients()
        {
            TutorialDataSO tData = _sessionData.tutorialData;
            return tData == null || tData.HasAllIngredients(_inventoryData);
        }

        public void AdvanceTutorial()
        {
            if (_sessionData == null || _sessionData.tutorialData == null) return;

            _sessionData.tutorialData.tutorialStep++;
            ApplyCurrentStep();
        }

        private void ApplyCurrentStep()
        {
            if (_sessionData == null || _sessionData.tutorialData == null) return;

            int currentStep = _sessionData.tutorialData.tutorialStep;

            if (currentStep < _tutorialSteps.Length)
            {
                _tutorialSteps[currentStep]?.Invoke();
            }
            else
            {
                Debug.Log("Tutorial Finalizado! Iniciando transição...");

                // Playtest Analytics ==================================
                PlaytestLogger.Instance.RecordTutorialFinished();
                // Playtest Analytics ==================================

                _sessionData.tutorialData = null;
                _sessionData.ResetSession();
                _inventoryData.ClearData();

                if (GameStateManager.Instance != null)
                {
                    GameStateManager.Instance.PopState();
                }

                string currentScene = SceneManager.GetActiveScene().name;

                if (FadeManager.Instance != null)
                {
                    FadeManager.Instance.StartTransition(currentScene, "BedSpawn", 0.5f, 1.5f);
                }
            }
        }

        public void InteractWithTutorialCounter()
        {
            if (_sessionData == null || _sessionData.tutorialData == null) return;

            int step = _sessionData.tutorialData.tutorialStep;

            if (step == 3 && _patientDelivery != null)
            {
                if (_sessionData.potionAwaitingDelivery && _inventoryData.craftedPotions.Count > 0)
                {
                    Sprite potionSprite = _inventoryData.craftedPotions[0].potionData.tableSprite;
                    _patientDelivery.ShowPotionButton(potionSprite, DeliverTutorialPotion);
                }
            }

            TutorialContext currentContext = EvaluateCounterContext();
            PlayTutorialDialogue(currentContext, true);
        }

        private void DeliverTutorialPotion()
        {
            if (_patientDelivery != null)
            {
                _patientDelivery.HidePotionButton();
            }

            if (_sessionData != null)
            {
                _sessionData.potionAwaitingDelivery = false;
            }

            if (_inventoryData != null)
            {
                _inventoryData.craftedPotions.Clear();
            }

            AdvanceTutorial();

            Debug.Log("Tocando diálogo de entrega de poção, avançando para o Step 4");
            PlayTutorialDialogue(TutorialContext.BALCAO_STEP_3, true);
        }

        public void InteractWithTutorialCauldron()
        {
            if (_sessionData == null || _sessionData.tutorialData == null) return;

            int step = _sessionData.tutorialData.tutorialStep;

            if (_spellButton != null)
            {
                if (step == 2) _spellButton.enabled = false;
                if (step == 4) _spellButton.enabled = true;
            }

            if (_cauldronController != null)
            {
                _cauldronController.onValidateIngredientDrop = step == 2 ? ValidateTutorialIngredient : null;

                _cauldronController.onValidateCauldronClick = ValidateCauldronClick;
                _cauldronController.onIngredientAdded = EvaluateIngredientCount;
                _cauldronController.onPotionFinished = EvaluatePotionFinished;
            }

            TutorialContext currentContext = EvaluateCauldronContext();
            PlayTutorialDialogue(currentContext, false);
        }

        private bool ValidateTutorialIngredient(PlantDataSO droppedPlant)
        {
            if (_sessionData == null || _sessionData.tutorialData == null) return true;

            if (!_sessionData.tutorialData.correctRecipe.Contains(droppedPlant))
            {
                Debug.Log("Jogador tentou colocar a planta errada no Tutorial!");
                PlayTutorialDialogue(TutorialContext.CALDEIRAO_ERRO_RECEITA, false);
                return false;
            }

            return true;
        }

        private void EvaluateIngredientCount(int currentCount)
        {
            if (_sessionData == null || _sessionData.tutorialData == null) return;

            int step = _sessionData.tutorialData.tutorialStep;

            if (step == 4 && currentCount == 3)
            {
                PlayTutorialDialogue(TutorialContext.CALDEIRAO_PRE_POCAO_STEP_4, false);
            }
        }

        private bool ValidateCauldronClick()
        {
            if (_sessionData == null || _sessionData.tutorialData == null) return true;

            int step = _sessionData.tutorialData.tutorialStep;

            if (step == 4)
            {
                PlayTutorialDialogue(TutorialContext.CALDEIRAO_ERRO_CLIQUE, false);
                return false;
            }

            return true;
        }

        private void EvaluatePotionFinished(bool wasSpellUsed)
        {
            if (_sessionData == null || _sessionData.tutorialData == null) return;

            int step = _sessionData.tutorialData.tutorialStep;

            if (step == 2)
            {
                Debug.Log("Poção normal feita com sucesso! Avançando pro Step 3.");
                PlayTutorialDialogue(TutorialContext.CALDEIRAO_FIM_STEP_2, false);
                AdvanceTutorial();
            }
            else if (step == 4)
            {
                Debug.Log("Poção mágica feita! Preparando Cutscene.");
                PlayTutorialDialogue(TutorialContext.CALDEIRAO_FIM_STEP_4, false);

                if (_exitButton != null)
                {
                    _exitButton.gameObject.SetActive(false);
                }
            }
        }

        private void PlayTutorialDialogue(TutorialContext context, bool showCharacterSprite)
        {
            if (_sessionData == null || _sessionData.tutorialData == null) return;

            Sprite agataSprite = showCharacterSprite ? _sessionData.tutorialData.agataPovSprite : null;

            if (agataSprite != null && _npcImage != null)
            {
                _npcImage.gameObject.SetActive(true);
                _npcImage.sprite = agataSprite;
                _npcImage.SetNativeSize();
            }
            else if (_npcImage != null)
            {
                _npcImage.gameObject.SetActive(false);
            }

            DialogueSequence seq = _sessionData.tutorialData.GetDialogue(context);
            if (seq.lines == null || seq.lines.Count == 0) return;

            Sprite agataBox = _sessionData.tutorialData.agataDialogueBox;
            Action onDialogueClosed = () => EvaluateTutorialAdvance(context);

            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.PlayDialogue(agataBox, seq.lines, onDialogueClosed, _sessionData.tutorialData.agataVoice);
            }
        }

        private void EvaluateTutorialAdvance(TutorialContext contextPlayed)
        {
            if (contextPlayed == TutorialContext.BALCAO_STEP_0)
            {
                Debug.Log("Terminou o diálogo inicial! Avançando para o Step 1.");
                AdvanceTutorial();
            }
            else if (contextPlayed == TutorialContext.CALDEIRAO_FIM_STEP_4)
            {
                AdvanceTutorial();

                if (_exitButton != null)
                {
                    _exitButton.onClick.Invoke();
                }

                StartTransformationCutscene();
            }
            else if (contextPlayed == TutorialContext.POS_CUTSCENE_STEP_5)
            {
                Debug.Log("Terminou o último diálogo, finalizando o tutorial.");

                if (DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.SetSortOrder(100);
                }

                AdvanceTutorial();
            }
        }

        private TutorialContext EvaluateCounterContext()
        {
            if (_sessionData == null || _sessionData.tutorialData == null) return TutorialContext.DEFAULT;

            int step = _sessionData.tutorialData.tutorialStep;
            if (step == 0) return TutorialContext.BALCAO_STEP_0;
            if (step == 1) return TutorialContext.BALCAO_STEP_1;

            return TutorialContext.DEFAULT;
        }

        private TutorialContext EvaluateCauldronContext()
        {
            if (_sessionData == null || _sessionData.tutorialData == null) return TutorialContext.DEFAULT;

            int step = _sessionData.tutorialData.tutorialStep;
            if (step == 2) return TutorialContext.CALDEIRAO_INICIO_STEP_2;
            if (step == 4) return TutorialContext.CALDEIRAO_INICIO_STEP_4;

            return TutorialContext.DEFAULT;
        }

    public void StartTransformationCutscene()
        {
            if (_transformationCoroutine != null)
            {
                StopCoroutine(_transformationCoroutine);
            }

            _transformationCoroutine = StartCoroutine(TransformationRoutine());
        }

        private IEnumerator TransformationRoutine()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.PushState(GameStateManager.GameState.CUTSCENE);
            }

            if (_playerController != null)
            {
                _playerController.ChangeIdleDirection(Vector2.left);
            }

            if (_vcam != null && _agataTopDownTransform != null)
            {
                _originalCameraTarget = _vcam.Follow;
                _vcam.Follow = _agataTopDownTransform;
            }

            const float shakeDuration = 2f;
            const float shakeIntensity = 0.1f;
            Vector3 originalPos = _agataTopDownTransform != null ? _agataTopDownTransform.position : Vector3.zero;
            float timer = 0f;

            while (timer < shakeDuration)
            {
                timer += Time.deltaTime;

                if (_agataTopDownTransform != null)
                {
                    float offsetX = UnityEngine.Random.Range(-1f, 1f) * shakeIntensity;
                    float offsetY = UnityEngine.Random.Range(-1f, 1f) * shakeIntensity;
                    _agataTopDownTransform.position = originalPos + new Vector3(offsetX, offsetY, 0f);
                }

                yield return null;
            }

            if (_agataTopDownTransform != null)
            {
                _agataTopDownTransform.position = originalPos;
            }

            if (_smokePrefab != null && _agataTopDownTransform != null)
            {
                Instantiate(_smokePrefab, _agataTopDownTransform.position + new Vector3(0f, -1.2f, 0f), Quaternion.identity);
            }

            if (_agataTopDownSpriteRenderer != null)
            {
                _agataTopDownSpriteRenderer.sprite = _agataCatSprite;
            }

            yield return new WaitForSeconds(2.5f);

            if (FadeManager.Instance != null)
            {
                FadeManager.Instance.StartFadeOut(1.5f, () =>
                {
                    if (_vcam != null)
                    {
                        _vcam.Follow = _originalCameraTarget;
                    }

                    if (DialogueManager.Instance != null)
                    {
                        DialogueManager.Instance.SetSortOrder(1001);
                    }

                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.StopMusic(1f);
                    }

                    PlayTutorialDialogue(TutorialContext.POS_CUTSCENE_STEP_5, false);
                });
            }

            _transformationCoroutine = null;
        }
    }
}
