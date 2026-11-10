using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JogoBruxinha.Core.UI
{
    public sealed class CursorHoverTrigger : MonoBehaviour, 
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        private Selectable _selectable;
        private bool _isPressed;

        private void Awake()
        {
            _selectable = GetComponent<Selectable>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_selectable != null && !_selectable.interactable) return;
            CursorManager.Instance?.SetHoverCursor();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_isPressed) 
                CursorManager.Instance?.SetDefaultCursor();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_selectable != null && !_selectable.interactable) return;
            CursorManager.Instance?.SetPressedCursor();

            _isPressed = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_selectable != null && !_selectable.interactable) return;

            _isPressed = false;
            
            if (eventData.hovered.Contains(gameObject))
                CursorManager.Instance?.SetHoverCursor();
            else
                CursorManager.Instance?.SetDefaultCursor();
        }

        private void OnDisable()
        {
            CursorManager.Instance?.SetDefaultCursor();
        }
    }
}