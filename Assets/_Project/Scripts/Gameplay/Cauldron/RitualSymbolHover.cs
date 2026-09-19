using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JogoBruxinha.Gameplay.Cauldron
{
    [RequireComponent(typeof(Image), typeof(Animator), typeof(Button))]
    public sealed class RitualSymbolHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Sprite _normalSymbol;
        private Image _image;
        private Animator _animator;
        private Button _button;
        private bool _hovered;
        private bool _animating;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _animator = GetComponent<Animator>();
            _button = GetComponent<Button>();
            // Selection and keyboard focus must not leave a gold animation paused.
            _button.transition = Selectable.Transition.None;
        }

        private void OnEnable() => ShowNormal();
        private void OnDisable()
        {
            _hovered = false;
            ShowNormal();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovered = true;
            Refresh();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovered = false;
            ShowNormal();
        }

        private void LateUpdate() => Refresh();

        private void Refresh()
        {
            bool animate = _hovered && _button.isActiveAndEnabled && _button.IsInteractable();
            if (animate == _animating) return;
            if (!animate) { ShowNormal(); return; }
            _animating = true;
            _animator.enabled = true;
            _animator.Play("Highlighted", 0, 0f);
            _animator.Update(0f);
        }

        private void ShowNormal()
        {
            if (_animator == null) return;
            _animating = false;
            _animator.enabled = false;
            _image.sprite = _normalSymbol;
            _image.color = Color.white;
        }
    }
}
