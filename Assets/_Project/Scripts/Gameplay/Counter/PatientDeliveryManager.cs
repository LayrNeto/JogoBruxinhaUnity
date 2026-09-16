using System;
using System.Collections.Generic;
using JogoBruxinha.Core.Audio;
using JogoBruxinha.Core.Events;
using JogoBruxinha.Gameplay.Characters.NPC;
using JogoBruxinha.Gameplay.Dialogue;
using JogoBruxinha.Gameplay.GameFlow;
using JogoBruxinha.Gameplay.Inventory;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace JogoBruxinha.Gameplay.Counter
{
    public sealed class PatientDeliveryManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button _potionButton;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Image _potionImage;
        [SerializeField] private Image _npcRenderer;

        [Header("Dependencies")]
        [SerializeField] private SessionDataSO _sessionData;
        [SerializeField] private InventoryDataSO _inventoryData;
        [SerializeField] private NPCBrain _npcBrain;

        [Header("Events")]
        [SerializeField] private GameEvent _nightFallEvent;

        [Header("Audio")]
        [SerializeField] private SoundDataSO _deliverPotionSound;
        [SerializeField] private SoundDataSO _healPatientSound;

        [Header("Delegates/Hooks")]
        public Action onPotionDelivery;

        private PatientDataSO _currentPatient;
        private bool _lastRecipeWasCorrect;
        private bool _isWaitingDelivery;

        // Propriedades para acesso direto caso necessário
        public Button PotionButton => _potionButton;
        public Image PotionImage => _potionImage;

        private void Awake()
        {
            Debug.Assert(_potionButton != null, $"Missing _potionButton reference on {name}", this);
            Debug.Assert(_restartButton != null, $"Missing _restartButton reference on {name}", this);
            Debug.Assert(_potionImage != null, $"Missing _potionImage reference on {name}", this);
            Debug.Assert(_npcRenderer != null, $"Missing _npcRenderer reference on {name}", this);
            Debug.Assert(_sessionData != null, $"Missing _sessionData reference on {name}", this);
            Debug.Assert(_inventoryData != null, $"Missing _inventoryData reference on {name}", this);
            Debug.Assert(_npcBrain != null, $"Missing _npcBrain reference on {name}", this);
        }

        private void Start()
        {
            if (GameStateManager.Instance != null && GameStateManager.Instance.InputControls != null)
            {
                GameStateManager.Instance.InputControls.UI.Close.performed += OnClosePerformed;
            }
        }

        private void OnDestroy()
        {
            if (GameStateManager.Instance != null && GameStateManager.Instance.InputControls != null)
            {
                GameStateManager.Instance.InputControls.UI.Close.performed -= OnClosePerformed;
            }
        }

        private void OnClosePerformed(InputAction.CallbackContext context)
        {
            ClosePOV();
        }

        public void StartPatientEncounter(PatientDataSO patient, List<DialogueLine> introDialogue)
        {
            _currentPatient = patient;
            _isWaitingDelivery = true;

            if (_npcRenderer != null && _currentPatient.povCursedSprite != null)
            {
                _npcRenderer.sprite = _currentPatient.povCursedSprite;
                _npcRenderer.SetNativeSize();
                _npcRenderer.gameObject.SetActive(true);
            }

            if (_sessionData.potionAwaitingDelivery && _inventoryData.craftedPotions.Count > 0)
            {
                Debug.Log("Entrou no balcão com poção pronta para ser entregue");
                ShowPotionButton(_inventoryData.craftedPotions[0].potionData.tableSprite);
            }
            else
            {
                Debug.Log("Sem poção pronta para entregar no balcão");
                HidePotionButton();
            }

            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.PlayDialogue(patient.dialogueBoxSprite, introDialogue, () =>
                {
                    if (_potionButton != null) _potionButton.enabled = true;
                });
            }
        }

        public void ShowPotionButton(Sprite sprite, Action deliveryAction = null)
        {
            if (_potionButton != null)
            {
                _potionButton.gameObject.SetActive(true);
                _potionButton.enabled = true;
            }

            if (_potionImage != null && sprite != null)
            {
                _potionImage.sprite = sprite;
            }

            if (deliveryAction != null)
            {
                onPotionDelivery = deliveryAction;
            }
        }

        public void HidePotionButton()
        {
            if (_potionButton != null)
            {
                _potionButton.gameObject.SetActive(false);
            }

            onPotionDelivery = null;
        }

        public void DeliverPotion()
        {
            if (AudioManager.Instance != null && _deliverPotionSound != null)
            {
                AudioManager.Instance.PlaySFX(_deliverPotionSound);
            }

            if (onPotionDelivery != null)
            {
                onPotionDelivery.Invoke();
                return;
            }

            HidePotionButton();
            _restartButton.gameObject.SetActive(false);
            _sessionData.potionAwaitingDelivery = false;
            _isWaitingDelivery = false;

            if (_inventoryData.craftedPotions.Count == 0)
            {
                Debug.LogWarning("Tentou entregar, mas não tem poção no inventário!");
                return;
            }

            BrewedPotion craftedPotion = _inventoryData.craftedPotions[0];
            bool isMagicPotion = craftedPotion.isSpellUsed;

            _lastRecipeWasCorrect = isMagicPotion || VerifyRecipe();

            if (_lastRecipeWasCorrect && _npcRenderer != null)
            {
                _npcRenderer.sprite = _currentPatient.povHealedSprite;
                _npcRenderer.SetNativeSize();

                if (AudioManager.Instance != null && _healPatientSound != null)
                {
                    AudioManager.Instance.PlaySFX(_healPatientSound);
                }
            }

            _inventoryData.craftedPotions.Clear();
            _inventoryData.savedInv[craftedPotion.potionData]--;

            List<DialogueLine> rawDialogue = _lastRecipeWasCorrect ? _currentPatient.successDialogue : _currentPatient.failDialogue;
            List<DialogueLine> filteredDialogue = FilterMagicDialogue(rawDialogue, isMagicPotion);

            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.PlayDialogue(_currentPatient.dialogueBoxSprite, filteredDialogue);
            }

            _sessionData.isNight = true;
            _nightFallEvent?.Raise();
        }

        public void ClosePOV()
        {
            if (!_isWaitingDelivery && _currentPatient != null && _npcBrain != null)
            {
                _npcBrain.LeaveShop(_lastRecipeWasCorrect);
            }

            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.ForceCloseDialogue();
            }

            gameObject.SetActive(false);
        }

        private bool VerifyRecipe()
        {
            List<PlantDataSO> potionIngredients = _inventoryData.craftedPotions[0].ingredients;
            List<PlantDataSO> patientRecipe = _currentPatient.requiredPlants;

            HashSet<PlantDataSO> potionSet = new HashSet<PlantDataSO>(potionIngredients);
            HashSet<PlantDataSO> recipeSet = new HashSet<PlantDataSO>(patientRecipe);

            return potionSet.SetEquals(recipeSet);
        }

        private List<DialogueLine> FilterMagicDialogue(List<DialogueLine> originalList, bool potionHasMagic)
        {
            List<DialogueLine> filteredList = new List<DialogueLine>();

            foreach (DialogueLine line in originalList)
            {
                if (line.requiresMagic && !potionHasMagic)
                {
                    continue;
                }
                filteredList.Add(line);
            }

            return filteredList;
        }

        public void RestartDialogue()
        {
            _sessionData.interactionCount = 1;

            if (DialogueManager.Instance != null && _currentPatient != null)
            {
                DialogueManager.Instance.PlayDialogue(_currentPatient.dialogueBoxSprite, _currentPatient.introDialogue, () =>
                {
                    if (_potionButton != null) _potionButton.enabled = true;
                });
            }
        }
    }
}