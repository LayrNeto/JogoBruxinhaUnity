using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using JogoBruxinha.Gameplay.Dialogue;
using JogoBruxinha.Core.SceneManagement;
using JogoBruxinha.Gameplay.Inventory;

namespace JogoBruxinha.Gameplay.Cauldron
{
    public sealed class IngredientSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [Header("References")]
        [SerializeField] private Image _backgroundImg;
        [SerializeField] private Image _foregroundImg;
        [SerializeField] private TextMeshProUGUI _amount;

        [Header("Ingredient Data")]
        [SerializeField] private PlantDataSO _plantData;

        private GameObject _ghostIcon;
        private Canvas _cauldronCanvas;
        private bool _dragging;
        private int _pointerId;
        private Pointer _pointer;
        private InputSystemUIInputModule _inputModule;
        private int _lastDragFrame = -1;
        public static IngredientSlot Selected { get; private set; }
        private static bool InputBlocked => (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying) ||
            (FadeManager.Instance != null && FadeManager.Instance.IsFading);

        public PlantDataSO PlantData => _plantData;
        public Image BackgroundImg => _backgroundImg;
        public Image ForegroundImg => _foregroundImg;
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
            if (eventData.button != PointerEventData.InputButton.Left || _dragging || InputBlocked) return;
            if (!PickUp(eventData)) return;
            _dragging = true;
            _pointerId = eventData.pointerId;
        }

        private bool PickUp(PointerEventData eventData)
        {
            if (_plantData == null || _plantData.dragSprite == null || _cauldronCanvas == null) return false;
            if (Selected != null) Selected.CancelSelection();
            Selected = this;
            _pointer = (eventData as ExtendedPointerEventData)?.device as Pointer;
            _inputModule = eventData.currentInputModule as InputSystemUIInputModule;

            _foregroundImg.enabled = false;
            _amount.enabled = false;

            _ghostIcon = new GameObject("ghostIcon", typeof(RectTransform), typeof(CanvasGroup));
            _ghostIcon.layer = _cauldronCanvas.gameObject.layer;
            _ghostIcon.transform.SetParent(_cauldronCanvas.transform, false);
            _ghostIcon.GetComponent<CanvasGroup>().blocksRaycasts = false;

            Image img = _ghostIcon.AddComponent<Image>();
            img.sprite = _plantData.dragSprite;
            img.raycastTarget = false;
            img.SetNativeSize();
            MoveGhost(eventData.position);
            return true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_dragging && eventData.button == PointerEventData.InputButton.Left && eventData.pointerId == _pointerId)
                MoveGhost(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_dragging || eventData.button != PointerEventData.InputButton.Left || eventData.pointerId != _pointerId) return;
            _lastDragFrame = Time.frameCount;
            CancelSelection();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || InputBlocked || _lastDragFrame == Time.frameCount) return;
            if (Selected == this) CancelSelection();
            else PickUp(eventData);
        }

        private void LateUpdate()
        {
            if (Selected != this) return;
            Mouse mouse = _pointer as Mouse;
            bool cancel = _inputModule != null && _inputModule.rightClick != null &&
                _inputModule.rightClick.action != null && _inputModule.rightClick.action.WasPressedThisFrame();
            if (InputBlocked || cancel || (mouse != null && mouse.rightButton.wasPressedThisFrame))
            {
                CancelSelection();
                return;
            }
            // Follow the pointer that picked this slot, not an unrelated Mouse.current.
            // Also update held drags every frame, after the EventSystem has processed input.
            if (_pointer != null && _pointer.added) MoveGhost(_pointer.position.ReadValue());
            else if (_inputModule != null && _inputModule.point != null && _inputModule.point.action != null)
                MoveGhost(_inputModule.point.action.ReadValue<Vector2>());
            else if (Mouse.current != null) MoveGhost(Mouse.current.position.ReadValue());
        }

        private void MoveGhost(Vector2 screenPosition)
        {
            if (_ghostIcon == null || _cauldronCanvas == null) return;
            Camera camera = _cauldronCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _cauldronCanvas.worldCamera;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)_cauldronCanvas.transform,
                screenPosition, camera, out Vector2 localPosition))
                _ghostIcon.transform.localPosition = localPosition;
        }

        public void CancelSelection()
        {
            if (_dragging) _lastDragFrame = Time.frameCount;
            if (_ghostIcon != null)
            {
                Destroy(_ghostIcon);
                _ghostIcon = null;
            }
            _dragging = false;
            _pointer = null;
            _inputModule = null;
            if (Selected == this) Selected = null;
            if (_foregroundImg != null) _foregroundImg.enabled = true;
            if (_amount != null) _amount.enabled = true;
        }

        private void OnDisable() => CancelSelection();

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus) CancelSelection();
        }
    }
}
