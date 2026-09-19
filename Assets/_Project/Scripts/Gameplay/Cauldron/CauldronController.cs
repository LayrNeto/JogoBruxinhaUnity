using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using JogoBruxinha.Core.Audio;
using JogoBruxinha.Gameplay.Inventory;
using JogoBruxinha.Gameplay.GameFlow;
using JogoBruxinha.Gameplay.Dialogue;
using JogoBruxinha.Core.SceneManagement;

namespace JogoBruxinha.Gameplay.Cauldron
{
    public sealed class CauldronController : MonoBehaviour, IDropHandler, IPointerClickHandler
    {
        [Header("Visual Components")]
        [SerializeField] private Animator _cauldronAnimator;
        [SerializeField] private Image[] _floatingIcons;
        [SerializeField] private PopupController _popup;
        [SerializeField] private IngredientManager _ingredientManager;
        [SerializeField] private GameObject _spellFilter;

        [Header("Potion Visual Tokens")]
        [SerializeField] private PotionDataSO _normalPotionVisual;
        [SerializeField] private PotionDataSO _magicPotionVisual;

        [Header("Game Data")]
        [SerializeField] private SessionDataSO _sessionData;
        [SerializeField] private InventoryDataSO _inventoryData;

        [Header("Audio")]
        [SerializeField] private SoundDataSO _spellCastedSound;
        [SerializeField] private SoundDataSO _errorSound;
        [SerializeField] private SoundDataSO _ingredientAddedSound;
        [SerializeField] private SoundDataSO _potionPreparedSound;

        [Header("Delegates/Hooks")]
        public Func<PlantDataSO, bool> onValidateIngredientDrop;
        public Func<bool> onValidateCauldronClick;
        public Action<int> onIngredientAdded;
        public Action<bool> onPotionFinished;

        private HashSet<PlantDataSO> _currentIngredients = new HashSet<PlantDataSO>();

        private const int MaxIngredients = 3;

        public bool HasEnoughIngredients => _currentIngredients.Count >= MaxIngredients;
        public Image CauldronImage => _cauldronAnimator != null ? _cauldronAnimator.GetComponent<Image>() : null;
        public bool ContainsIngredient(PlantDataSO plant) => _currentIngredients.Contains(plant);

        private static readonly int PotionReadyHash = Animator.StringToHash("PotionReady");
        private static readonly int SpellUsedHash = Animator.StringToHash("SpellUsed");
        private static readonly int PotionStartedHash = Animator.StringToHash("PotionStarted");
        private static readonly int CauldronSpellUsedStateHash = Animator.StringToHash("cauldronSpellUsed");
        private static readonly int CauldronPotionFinishedStateHash = Animator.StringToHash("cauldronPotionFinished");

        private void Awake()
        {
            // Receive drops on the visible cauldron, including areas outside the old hitbox.
            if (CauldronImage != null) CauldronImage.raycastTarget = true;
            Debug.Assert(_cauldronAnimator != null, $"Missing _cauldronAnimator reference on {name}", this);
            Debug.Assert(_popup != null, $"Missing _popup reference on {name}", this);
            Debug.Assert(_ingredientManager != null, $"Missing _ingredientManager reference on {name}", this);
            Debug.Assert(_normalPotionVisual != null, $"Missing _normalPotionVisual reference on {name}", this);
            Debug.Assert(_magicPotionVisual != null, $"Missing _magicPotionVisual reference on {name}", this);
            Debug.Assert(_sessionData != null, $"Missing _sessionData reference on {name}", this);
            Debug.Assert(_inventoryData != null, $"Missing _inventoryData reference on {name}", this);
        }

        private void OnEnable()
        {
            if (_sessionData != null && _sessionData.potionAwaitingDelivery)
            {
                if (_cauldronAnimator != null)
                {
                    bool wasSpellUsed = false;

                    if (_inventoryData != null && _inventoryData.craftedPotions.Count > 0)
                    {
                        wasSpellUsed = _inventoryData.craftedPotions[0].isSpellUsed;
                    }

                    _cauldronAnimator.SetBool(PotionReadyHash, true);
                    _cauldronAnimator.SetBool(SpellUsedHash, wasSpellUsed);

                    int targetStateHash = wasSpellUsed ? CauldronSpellUsedStateHash : CauldronPotionFinishedStateHash;
                    _cauldronAnimator.Play(targetStateHash);
                }
            }
            else
            {
                _currentIngredients.Clear();
                ResetVisuals();
            }

            if (_popup != null)
            {
                _popup.gameObject.SetActive(false);
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || eventData.pointerDrag == null) return;
            IngredientSlot draggedSlot = eventData.pointerDrag.GetComponent<IngredientSlot>();
            if (draggedSlot == null || IngredientSlot.Selected != draggedSlot) return;
            TryAddIngredient(draggedSlot.PlantData);
            draggedSlot.CancelSelection();
        }

        private static bool InputBlocked => (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying) ||
            (FadeManager.Instance != null && FadeManager.Instance.IsFading);

        public bool TryAddIngredient(PlantDataSO plant)
        {
            if (plant == null || InputBlocked) return false;
            if (_sessionData != null && _sessionData.potionAwaitingDelivery)
            {
                ShowErrorMessage("Já existe uma poção no balcão");
                return false;
            }

            if (_inventoryData == null || !_inventoryData.savedInv.TryGetValue(plant, out int quantity) || quantity < 1) return false;
            if (onValidateIngredientDrop != null && !onValidateIngredientDrop.Invoke(plant)) return false;

            if (_currentIngredients.Count >= MaxIngredients)
            {
                ShowErrorMessage("O caldeirao já está cheio");
                return false;
            }

            if (_currentIngredients.Add(plant))
            {
                if (AudioManager.Instance != null && _ingredientAddedSound != null)
                {
                    AudioManager.Instance.PlaySFX(_ingredientAddedSound);
                }

                bool hasBg = plant.emptyShelfSprite != null;
                UpdateFloatingIcons(plant.dragSprite, hasBg);

                string message = (_currentIngredients.Count == MaxIngredients) 
                    ? "Clique no caldeirão para engarrafar a poção" 
                    : "Ingrediente adicionado";

                if (_popup != null) _popup.ShowMessage(message);

                if (_cauldronAnimator != null)
                {
                    _cauldronAnimator.SetBool(PotionStartedHash, true);
                }

                onIngredientAdded?.Invoke(_currentIngredients.Count);
                return true;
            }
            else
            {
                ShowErrorMessage("Ingrediente repetido");
                return false;
            }
        }

        private void UpdateFloatingIcons(Sprite ingredientSprite, bool hasBg)
        {
            if (_floatingIcons == null) return;

            for (int i = 0; i < _floatingIcons.Length; i++)
            {
                if (_floatingIcons[i] == null) continue;

                if (!_floatingIcons[i].enabled)
                {
                    if (hasBg)
                    {
                        _floatingIcons[i].gameObject.transform.localScale = Vector3.one;
                    }

                    _floatingIcons[i].sprite = ingredientSprite;
                    _floatingIcons[i].enabled = true;
                    _floatingIcons[i].SetNativeSize();
                    break;
                }
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || InputBlocked) return;
            if (IngredientSlot.Selected != null)
            {
                IngredientSlot selected = IngredientSlot.Selected;
                TryAddIngredient(selected.PlantData);
                selected.CancelSelection();
                return;
            }
            if (_sessionData != null && _sessionData.potionAwaitingDelivery)
            {
                ShowErrorMessage("Leve primeiro a poção ao balcão");
                return;
            }

            if (onValidateCauldronClick != null && !onValidateCauldronClick.Invoke()) return;

            if (_currentIngredients.Count == MaxIngredients)
            {
                FinishPotion(false);
            }
            else
            {
                ShowErrorMessage($"Adicione {MaxIngredients} ingredientes primeiro");
            }
        }

        public void BrewWithMagic()
        {
            if (InputBlocked) return;
            if (_sessionData != null && _sessionData.potionAwaitingDelivery)
            {
                ShowErrorMessage("Leve primeiro a poção ao balcão");
                return;
            }

            if (_currentIngredients.Count == MaxIngredients)
            {
                FinishPotion(true);
            }
            else
            {
                ShowErrorMessage($"Adicione {MaxIngredients} ingredientes primeiro");
            }
        }
        private void FinishPotion(bool isSpellUsed)
        {
            if (_inventoryData == null || !_inventoryData.HasSpaceForPotion())
            {
                ShowErrorMessage("Sem espaço no inventário para a poção");
                return;
            }
            if (isSpellUsed && (_sessionData == null || !_sessionData.TrySpendRitualEnergy()))
            {
                ShowErrorMessage($"O ritual precisa de {SessionDataSO.RitualEnergyCost} de energia vital");
                return;
            }

            if (isSpellUsed)
            {
                if (AudioManager.Instance != null && _spellCastedSound != null)
                    AudioManager.Instance.PlaySFX(_spellCastedSound);
                if (_spellFilter != null) _spellFilter.SetActive(true);
            }
            if (AudioManager.Instance != null && _potionPreparedSound != null)
            {
                AudioManager.Instance.PlaySFX(_potionPreparedSound);
            }

            if (_sessionData != null)
            {
                _sessionData.potionAwaitingDelivery = true;
            }

            PotionDataSO visualToken = isSpellUsed ? _magicPotionVisual : _normalPotionVisual;
            BrewedPotion potion = new BrewedPotion(_currentIngredients, isSpellUsed, visualToken);

            if (_inventoryData != null && _inventoryData.HasSpaceForPotion())
            {
                _inventoryData.AddPotion(potion);

                foreach (ItemDataSO item in _currentIngredients)
                {
                    if (_inventoryData.savedInv.ContainsKey(item))
                    {
                        _inventoryData.savedInv[item] -= 1;
                    }
                }

                if (_ingredientManager != null)
                {
                    _ingredientManager.UpdateShelves();
                }
            }

            _currentIngredients.Clear();
            ResetVisuals();

            if (_cauldronAnimator != null)
            {
                _cauldronAnimator.SetBool(PotionReadyHash, true);
                _cauldronAnimator.SetBool(SpellUsedHash, isSpellUsed);
            }

            if (_popup != null)
            {
                _popup.ShowMessage("Poção enviada para o inventário");
            }

            onPotionFinished?.Invoke(isSpellUsed);
        }

        private void ResetVisuals()
        {
            if (_floatingIcons != null)
            {
                foreach (Image icon in _floatingIcons)
                {
                    if (icon == null) continue;
                    icon.enabled = false;
                    icon.sprite = null;
                }
            }

            if (_cauldronAnimator != null)
            {
                _cauldronAnimator.SetBool(PotionStartedHash, false);
            }
        }

        private void ShowErrorMessage(string popupText)
        {
            if (AudioManager.Instance != null && _errorSound != null)
            {
                AudioManager.Instance.PlaySFX(_errorSound);
            }

            if (_popup != null)
            {
                _popup.ShowMessage(popupText);
            }
        }
    }
}
