using JogoBruxinha.Core.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace JogoBruxinha.Gameplay.Cauldron
{
    public sealed class BookManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _contentImage;
        [SerializeField] private GameObject _tableSupport;
        [SerializeField] private GameObject _nextButton;
        [SerializeField] private GameObject _previousButton;
        [SerializeField] private GameObject _ritualSymbol;

        [Header("Audio")]
        [SerializeField] private SoundDataSO _turningPageSound;
        [SerializeField] private SoundDataSO _openBookSound;

        [Header("Pages")]
        [SerializeField] private Sprite[] _pages;

        private int _currentPage = 0;

        public GameObject TableBook => _tableSupport;
        public GameObject NextPageButton => _nextButton;
        public GameObject RitualSymbol => _ritualSymbol;
        public bool IsOpen => gameObject.activeInHierarchy;

        private void Awake()
        {
            Debug.Assert(_contentImage != null, $"Missing _contentImage reference on {name}", this);
            Debug.Assert(_tableSupport != null, $"Missing _tableSupport reference on {name}", this);
            Debug.Assert(_nextButton != null, $"Missing _nextButton reference on {name}", this);
            Debug.Assert(_previousButton != null, $"Missing _previousButton reference on {name}", this);
            Debug.Assert(_ritualSymbol != null, $"Missing _ritualSymbol reference on {name}", this);
            Debug.Assert(_pages != null && _pages.Length > 0, $"Missing _pages array or empty on {name}", this);
        }

        private void Start()
        {
            if (_pages != null && _pages.Length > 0)
            {
                UpdatePage();
            }
        }

        public void NextPage()
        {
            if (_pages == null) return;

            if (_currentPage < _pages.Length - 1)
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

        public void OpenBook()
        {
            if (AudioManager.Instance != null && _openBookSound != null)
            {
                AudioManager.Instance.PlaySFX(_openBookSound);
            }

            if (_tableSupport != null)
            {
                _tableSupport.SetActive(true);
            }
        }

        private void UpdatePage()
        {
            _contentImage.sprite = _pages[_currentPage];
            _nextButton.SetActive(_currentPage < _pages.Length - 1);
            _previousButton.SetActive(_currentPage > 0);
            _ritualSymbol.SetActive(_currentPage == _pages.Length - 1);

            if (AudioManager.Instance != null && _turningPageSound != null)
            {
                AudioManager.Instance.PlaySFX(_turningPageSound);
            }
        }
    }
}
