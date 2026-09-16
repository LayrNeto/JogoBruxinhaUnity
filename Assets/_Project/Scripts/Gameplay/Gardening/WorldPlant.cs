using JogoBruxinha.Core.Audio;
using JogoBruxinha.Gameplay.Environment;
using JogoBruxinha.Gameplay.Events;
using JogoBruxinha.Gameplay.GameFlow;
using JogoBruxinha.Gameplay.Inventory;
using UnityEngine;

namespace JogoBruxinha.Gameplay.Gardening
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Interactable))]
    public sealed class WorldPlant : MonoBehaviour
    {
        [Header("Plant Data")]
        [SerializeField] private PlantDataSO _plantData;
        [SerializeField] private string _uniqueInstanceID;

        [Header("Dependencies")]
        [SerializeField] private SessionDataSO _sessionData;

        [Header("Collect Event")]
        [SerializeField] private GameEventItemStruct _collectEvent;

        private Interactable _interactableComponent;
        private SpriteRenderer _spriteRenderer;

        private int _visualIndex;
        private int _soundIndex;
        private bool _isCollected;

        public PlantDataSO PlantData => _plantData;
        public string UniqueInstanceID => _uniqueInstanceID;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _interactableComponent = GetComponent<Interactable>();

            Debug.Assert(_spriteRenderer != null, $"Missing SpriteRenderer component on {name}", this);
            Debug.Assert(_interactableComponent != null, $"Missing Interactable component on {name}", this);
            Debug.Assert(_plantData != null, $"Missing _plantData reference on {name}", this);
            Debug.Assert(_sessionData != null, $"Missing _sessionData reference on {name}", this);
            Debug.Assert(_collectEvent != null, $"Missing _collectEvent reference on {name}", this);
            Debug.Assert(!string.IsNullOrEmpty(_uniqueInstanceID), $"Missing _uniqueInstanceID on {name}", this);
        }

        private void Start()
        {
            if (_sessionData == null || string.IsNullOrEmpty(_uniqueInstanceID)) return;

            if (_sessionData.plantDaysRemaining.TryGetValue(_uniqueInstanceID, out int daysRemaining) && daysRemaining > 0)
            {
                SetupCollectedPlant();
            }
            else
            {
                SetupAlivePlant();
            }
        }

        private void SetupAlivePlant()
        {
            _isCollected = false;

            if (_interactableComponent != null)
            {
                _interactableComponent.canInteract = true;
            }

            if (_plantData == null) return;

            if (_plantData.collectSound != null && _plantData.collectSound.Length > 0)
            {
                _soundIndex = Random.Range(0, _plantData.collectSound.Length);
            }

            if (_plantData.worldSprites != null && _plantData.worldSprites.Length > 0)
            {
                _visualIndex = Random.Range(0, _plantData.worldSprites.Length);
                _spriteRenderer.sprite = _plantData.worldSprites[_visualIndex];
            }
        }

        private void SetupCollectedPlant()
        {
            _isCollected = true;

            if (_interactableComponent != null)
            {
                _interactableComponent.canInteract = false;
            }

            if (_plantData == null || _plantData.collectedSprites == null || _plantData.collectedSprites.Length == 0) return;

            int safeIndex = Mathf.Clamp(_visualIndex, 0, _plantData.collectedSprites.Length - 1);
            _spriteRenderer.sprite = _plantData.collectedSprites[safeIndex];
        }

        public void Collect()
        {
            if (_isCollected || _plantData == null) return;

            _isCollected = true;

            if (_interactableComponent != null)
            {
                _interactableComponent.canInteract = false;
            }

            if (_plantData.collectedSprites != null && _plantData.collectedSprites.Length > 0)
            {
                int safeIndex = Mathf.Clamp(_visualIndex, 0, _plantData.collectedSprites.Length - 1);
                _spriteRenderer.sprite = _plantData.collectedSprites[safeIndex];
            }

            if (_sessionData != null)
            {
                _sessionData.SetPlantGrowth(_uniqueInstanceID, _plantData.daysToGrow);
            }

            _collectEvent?.Raise(new ItemStruct(_plantData, 1));

            if (AudioManager.Instance != null && 
                _plantData.collectSound != null && 
                _soundIndex >= 0 && 
                _soundIndex < _plantData.collectSound.Length)
            {
                AudioManager.Instance.PlaySFX(_plantData.collectSound[_soundIndex]);
            }
        }
    }
}