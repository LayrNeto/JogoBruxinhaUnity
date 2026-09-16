using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using JogoBruxinha.Gameplay.Inventory;

namespace JogoBruxinha.Gameplay.Cauldron
{
    public sealed class IngredientSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("References")]
        [SerializeField] private Image _backgroundImg;
        [SerializeField] private Image _foregroundImg;
        [SerializeField] private TextMeshProUGUI _amount;

        [Header("Ingredient Data")]
        [SerializeField] private PlantDataSO _plantData;

        private GameObject _ghostIcon;
        private Canvas _cauldronCanvas;

        public PlantDataSO PlantData => _plantData;
        public Image BackgroundImg => _backgroundImg;
        public TextMeshProUGUI Amount => _amount;

        private void Awake()
        {
            _cauldronCanvas = GetComponentInParent<Canvas>();

            Debug.Assert(_backgroundImg != null, $"Missing _backgroundImg reference on {name}", this);
            Debug.Assert(_foregroundImg != null, $"Missing _foregroundImg reference on {name}", this);
            Debug.Assert(_amount != null, $"Missing _amount reference on {name}", this);
            Debug.Assert(_cauldronCanvas != null, $"Missing parent Canvas on {name}", this);
        }

        private void Start()
        {
            if (_plantData != null)
            {
                _foregroundImg.sprite = _plantData.shelfSprite;

                if (_plantData.emptyShelfSprite != null)
                {
                    _backgroundImg.gameObject.SetActive(true);
                    _backgroundImg.sprite = _plantData.emptyShelfSprite;
                }
                else
                {
                    _backgroundImg.gameObject.SetActive(false);
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_plantData == null || _cauldronCanvas == null) return;

            _foregroundImg.enabled = false;
            _amount.enabled = false;

            _ghostIcon = new GameObject("ghostIcon");
            _ghostIcon.transform.SetParent(_cauldronCanvas.transform, false);

            Image img = _ghostIcon.AddComponent<Image>();
            img.sprite = _plantData.dragSprite;
            img.raycastTarget = false;
            img.SetNativeSize();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_ghostIcon != null)
            {
                _ghostIcon.transform.position = eventData.position;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_ghostIcon != null)
            {
                Destroy(_ghostIcon);
            }

            _foregroundImg.enabled = true;
            _amount.enabled = true;
        }
    }
}