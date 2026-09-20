using System;
using System.Collections.Generic;
using JogoBruxinha.Core.Audio;
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

        [Header("Typewriter")]
        [SerializeField, Min(1f)] private float _charactersPerSecond = 32f;
        [SerializeField, Min(0f)] private float _punctuationDelay = 0.12f;
        [SerializeField] private SoundDataSO _playerVoice;
        [Tooltip("Global voice multiplier, applied to each voice asset's Volume.")]
        [SerializeField, Range(0f, 1f)] private float _voiceVolume = 0.6f;

        private AudioSource _voiceSource;
        private DialogueVoicePlayback _voicePlayback;
        private SoundDataSO _npcVoice;
        private SoundDataSO _currentVoice;
        private GameStateManager _inputOwner;
        private int _visibleCharacters;
        private int _lastAdvanceFrame = -1;
        private float _nextCharacterTime;
        public bool IsPlaying { get; private set; }
        public bool IsTyping { get; private set; }

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
                _voiceSource = gameObject.AddComponent<AudioSource>();
                _voiceSource.playOnAwake = false;
                _voiceSource.loop = false;
                _voiceSource.spatialBlend = 0f;
                _voicePlayback = new DialogueVoicePlayback(_voiceSource);
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
            SubscribeInput();
        }

        private void Start()
        {
            SubscribeInput();
        }

        private void SubscribeInput()
        {
            if (_inputOwner == null && GameStateManager.Instance != null && GameStateManager.Instance.InputControls != null)
            {
                _inputOwner = GameStateManager.Instance;
                _inputOwner.InputControls.UI.SkipDialogue.performed += OnSkipDialoguePerformed;
                _inputOwner.InputControls.UI.ScrollWheel.performed += OnScroll;
            }
        }

        private void OnDisable()
        {
            StopTyping();
            IsPlaying = false;
            _currentDialogueList = null;
            _dynamicOnFinishCallback = null;
            if (_inputOwner != null && _inputOwner.InputControls != null)
            {
                _inputOwner.InputControls.UI.SkipDialogue.performed -= OnSkipDialoguePerformed;
                _inputOwner.InputControls.UI.ScrollWheel.performed -= OnScroll;
            }
            _inputOwner = null;
        }

        private void OnDestroy()
        {
            _voicePlayback?.Dispose();
            if (Instance == this) Instance = null;
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

        public void PlayDialogue(Sprite boxSprite, List<DialogueLine> dialogue, Action onFinish = null, SoundDataSO npcVoice = null)
        {
            ForceCloseDialogue();
            if (dialogue == null || dialogue.Count == 0)
            {
                onFinish?.Invoke();
                return;
            }

            _currentDialogueList = dialogue;
            _currentNPCBox = boxSprite;
            _npcVoice = npcVoice;
            _currentLineIndex = 0;
            _maxLineIndex = 0;
            _dynamicOnFinishCallback = onFinish;

            if (_dialogueCanvasRoot != null) _dialogueCanvasRoot.SetActive(true);
            if (_nextButton != null) _nextButton.SetActive(true);
            if (_dialogueBackgroundImage != null) _dialogueBackgroundImage.gameObject.SetActive(true);

            IsPlaying = true;
            _lastAdvanceFrame = Time.frameCount;
            DisplayCurrentLine(true);
        }

        public void ShowNextLine()
        {
            if (!IsPlaying || _currentDialogueList == null || _lastAdvanceFrame == Time.frameCount) return;
            _lastAdvanceFrame = Time.frameCount;

            // A single input completes the text OR advances; never both.
            if (IsTyping)
            {
                StopTyping();
                _dialogueText.maxVisibleCharacters = int.MaxValue;
                return;
            }

            _currentLineIndex++;

            if (_currentLineIndex < _currentDialogueList.Count)
            {
                bool unread = _currentLineIndex > _maxLineIndex;
                _maxLineIndex = Mathf.Max(_maxLineIndex, _currentLineIndex);
                DisplayCurrentLine(unread);
            }
            else
            {
                FinishDialogue();
            }
        }

        public void ShowPreviousLine()
        {
            if (!IsPlaying || _currentDialogueList == null || _lastAdvanceFrame == Time.frameCount) return;
            _lastAdvanceFrame = Time.frameCount;

            if (_currentLineIndex > 0)
            {
                _currentLineIndex--;

                DisplayCurrentLine(false);
            }
        }

        private void DisplayCurrentLine(bool animate)
        {
            StopTyping();
            DialogueLine line = _currentDialogueList[_currentLineIndex];
            _currentVoice = line.speaker == SpeakerType.Player ? _playerVoice : _npcVoice;
            _voicePlayback?.Prepare(_currentVoice);
            _dialogueBackgroundImage.sprite = line.speaker == SpeakerType.Player ? _playerDialogueBox : _currentNPCBox;
            _dialogueText.text = line.text ?? string.Empty;
            // Lay out the full paragraph before revealing it, avoiding jumping words.
            _dialogueText.maxVisibleCharacters = int.MaxValue;
            _dialogueText.ForceMeshUpdate();
            _visibleCharacters = 0;
            IsTyping = animate && _dialogueText.textInfo.characterCount > 0;
            _dialogueText.maxVisibleCharacters = IsTyping ? 0 : int.MaxValue;
            _nextCharacterTime = Time.unscaledTime;
        }

        private void Update()
        {
            if (!IsPlaying || !IsTyping) return;

            int count = _dialogueText.textInfo.characterCount;
            bool playVoice = false;
            while (_visibleCharacters < count && Time.unscaledTime >= _nextCharacterTime)
            {
                char c = _dialogueText.textInfo.characterInfo[_visibleCharacters].character;
                _visibleCharacters++;
                if (char.IsWhiteSpace(c)) continue;
                playVoice |= char.IsLetterOrDigit(c);
                _nextCharacterTime += 1f / Mathf.Max(1f, _charactersPerSecond);
                if (char.IsPunctuation(c) && (_visibleCharacters == count ||
                    !char.IsPunctuation(_dialogueText.textInfo.characterInfo[_visibleCharacters].character)))
                    _nextCharacterTime += _punctuationDelay;
            }

            _dialogueText.maxVisibleCharacters = _visibleCharacters;
            if (playVoice) _voicePlayback?.TryPlay(_currentVoice, _voiceVolume);
            if (_visibleCharacters >= count) IsTyping = false;
        }

        private void StopTyping()
        {
            IsTyping = false;
            // No queued syllables: let only the current short blip finish its release envelope.
        }

        private void FinishDialogue()
        {
            Action callback = _dynamicOnFinishCallback;
            ForceCloseDialogue();
            callback?.Invoke();
        }

        public void ForceCloseDialogue()
        {
            StopTyping();
            IsPlaying = false;
            _currentDialogueList = null;
            _dynamicOnFinishCallback = null;
            if (_dialogueCanvasRoot != null) _dialogueCanvasRoot.SetActive(false);
            if (_nextButton != null) _nextButton.SetActive(false);
            if (_dialogueBackgroundImage != null) _dialogueBackgroundImage.gameObject.SetActive(false);
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
