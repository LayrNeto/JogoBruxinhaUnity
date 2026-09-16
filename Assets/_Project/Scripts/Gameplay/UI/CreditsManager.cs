using JogoBruxinha.Gameplay.GameFlow;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace JogoBruxinha.Gameplay.UI
{
    public sealed class CreditsManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _contentImage;
        [SerializeField] private GameObject _nextButton;
        [SerializeField] private GameObject _previousButton;

        [Header("Screens")]
        [SerializeField] private Sprite[] _screens = new Sprite[0];

        private int _currentPage = 0;

        private void Awake()
        {
            Debug.Assert(_contentImage != null, $"Missing _contentImage reference on {name}", this);
            Debug.Assert(_nextButton != null, $"Missing _nextButton reference on {name}", this);
            Debug.Assert(_previousButton != null, $"Missing _previousButton reference on {name}", this);
            Debug.Assert(_screens != null && _screens.Length > 0, $"Missing or empty _screens array on {name}", this);
        }

        private void OnEnable()
        {
            if (GameStateManager.Instance != null && GameStateManager.Instance.InputControls != null)
            {
                GameStateManager.Instance.InputControls.UI.Close.performed += OnCloseCredits;
            }

            UpdatePage();
        }

        private void OnDisable()
        {
            if (GameStateManager.Instance != null && GameStateManager.Instance.InputControls != null)
            {
                GameStateManager.Instance.InputControls.UI.Close.performed -= OnCloseCredits;
            }
        }

        private void OnCloseCredits(InputAction.CallbackContext ctx)
        {
            CloseCredits();
        }

        public void NextPage()
        {
            if (_screens == null || _screens.Length == 0) return;

            if (_currentPage < _screens.Length - 1)
            {
                _currentPage++;
                UpdatePage();
            }
        }

        public void PreviousPage()
        {
            if (_currentPage > 0)
            {
                _currentPage--;
                UpdatePage();
            }
        }

        private void UpdatePage()
        {
            if (_screens == null || _screens.Length == 0 || _contentImage == null) return;

            _currentPage = Mathf.Clamp(_currentPage, 0, _screens.Length - 1);
            _contentImage.sprite = _screens[_currentPage];

            if (_nextButton != null)
            {
                _nextButton.SetActive(_currentPage < _screens.Length - 1);
            }

            if (_previousButton != null)
            {
                _previousButton.SetActive(_currentPage > 0);
            }
        }

        public void CloseCredits()
        {
            _currentPage = 0;
            UpdatePage();
            gameObject.SetActive(false);
        }
    }
}