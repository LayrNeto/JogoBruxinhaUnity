using System;
using System.Collections.Generic;
using JogoBruxinha.Gameplay.Counter;
using JogoBruxinha.Gameplay.GameFlow;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace JogoBruxinha.Gameplay.Dialogue
{
    public sealed class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject _dialogueCanvasRoot;
        [SerializeField] private GameObject _nextButton;
        [SerializeField] private Image _dialogueBackgroundImage;
        [SerializeField] private TextMeshProUGUI _dialogueText;

        [Header("Assets")]
        [SerializeField] private Sprite _playerDialogueBox;

        private Sprite _currentNPCBox;
        private List<DialogueLine> _currentDialogueList;
        private int _currentLineIndex = -1;
        private int _maxLineIndex = 0;
        private Action _dynamicOnFinishCallback;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                ValidateDependencies();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void ValidateDependencies()
        {
            Debug.Assert(_dialogueCanvasRoot != null, $"Missing _dialogueCanvasRoot reference on {name}", this);
            Debug.Assert(_nextButton != null, $"Missing _nextButton reference on {name}", this);
            Debug.Assert(_dialogueBackgroundImage != null, $"Missing _dialogueBackgroundImage reference on {name}", this);
            Debug.Assert(_dialogueText != null, $"Missing _dialogueText reference on {name}", this);
            Debug.Assert(_playerDialogueBox != null, $"Missing _playerDialogueBox reference on {name}", this);
        }

        private void OnEnable()
        {
            if (GameStateManager.Instance != null && GameStateManager.Instance.InputControls != null)
            {
                GameStateManager.Instance.InputControls.UI.SkipDialogue.performed += OnSkipDialoguePerformed;
                GameStateManager.Instance.InputControls.UI.ScrollWheel.performed += OnScroll;
            }
        }

        private void OnDisable()
        {
            if (GameStateManager.Instance != null && GameStateManager.Instance.InputControls != null)
            {
                GameStateManager.Instance.InputControls.UI.SkipDialogue.performed -= OnSkipDialoguePerformed;
                GameStateManager.Instance.InputControls.UI.ScrollWheel.performed -= OnScroll;
            }
        }

        private void OnScroll(InputAction.CallbackContext ctx)
        {
            Vector2 scrollDir = ctx.ReadValue<Vector2>();

            if (scrollDir.y < 0f)
            {
                ShowPreviousLine();
            }
            else if (scrollDir.y > 0f && _currentLineIndex < _maxLineIndex)
            {
                ShowNextLine();
            }
        }

        private void OnSkipDialoguePerformed(InputAction.CallbackContext ctx)
        {
            ShowNextLine();
        }

        public void PlayDialogue(Sprite boxSprite, List<DialogueLine> dialogue, Action onFinish = null)
        {
            if (dialogue == null || dialogue.Count == 0)
            {
                onFinish?.Invoke();
                return;
            }

            _currentDialogueList = dialogue;
            _currentNPCBox = boxSprite;
            _currentLineIndex = -1;
            _maxLineIndex = 0;
            _dynamicOnFinishCallback = onFinish;

            if (_dialogueCanvasRoot != null) _dialogueCanvasRoot.SetActive(true);
            if (_nextButton != null) _nextButton.SetActive(true);
            if (_dialogueBackgroundImage != null) _dialogueBackgroundImage.gameObject.SetActive(true);

            ShowNextLine();
        }

        public void ShowNextLine()
        {
            if (_currentDialogueList == null || _currentDialogueList.Count == 0)
            {
                FinishDialogue();
                return;
            }

            _currentLineIndex++;

            if (_currentLineIndex < _currentDialogueList.Count)
            {
                if (_currentLineIndex > _maxLineIndex)
                {
                    _maxLineIndex = _currentLineIndex;
                }

                DialogueLine currentLine = _currentDialogueList[_currentLineIndex];
                _dialogueText.text = currentLine.text;
                _dialogueBackgroundImage.sprite = (currentLine.speaker == SpeakerType.Player) 
                    ? _playerDialogueBox 
                    : _currentNPCBox;
            }
            else
            {
                FinishDialogue();
            }
        }

        public void ShowPreviousLine()
        {
            if (_currentDialogueList == null) return;

            if (_currentLineIndex > 0)
            {
                _currentLineIndex--;

                DialogueLine currentLine = _currentDialogueList[_currentLineIndex];
                _dialogueText.text = currentLine.text;
                _dialogueBackgroundImage.sprite = (currentLine.speaker == SpeakerType.Player) 
                    ? _playerDialogueBox 
                    : _currentNPCBox;
            }
        }

        private void FinishDialogue()
        {
            if (_dialogueCanvasRoot != null) _dialogueCanvasRoot.SetActive(false);
            if (_nextButton != null) _nextButton.SetActive(false);
            if (_dialogueBackgroundImage != null) _dialogueBackgroundImage.gameObject.SetActive(false);

            Action callback = _dynamicOnFinishCallback;
            _dynamicOnFinishCallback = null;
            callback?.Invoke();
        }

        public void ForceCloseDialogue()
        {
            if (_dialogueCanvasRoot != null) _dialogueCanvasRoot.SetActive(false);
            if (_dialogueBackgroundImage != null) _dialogueBackgroundImage.gameObject.SetActive(false);

            _dynamicOnFinishCallback = null;
        }

        public void SetSortOrder(int newOrder)
        {
            if (_dialogueCanvasRoot != null)
            {
                Canvas canvas = _dialogueCanvasRoot.GetComponent<Canvas>();
                if (canvas != null)
                {
                    canvas.sortingOrder = newOrder;
                }
            }
        }
    }
}