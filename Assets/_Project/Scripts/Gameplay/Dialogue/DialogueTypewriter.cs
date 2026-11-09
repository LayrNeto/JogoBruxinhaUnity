using JogoBruxinha.Core.Audio;
using TMPro;
using UnityEngine;

namespace JogoBruxinha.Gameplay.Dialogue
{
    public sealed class DialogueTypewriter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI _textComponent;

        [Header("Settings")]
        [SerializeField, Min(1f)] private float _charactersPerSecond = 32f;
        [SerializeField, Min(0f)] private float _punctuationDelay = 0.12f;
        [SerializeField, Range(0f, 1f)] private float _voiceVolume = 0.6f;

        public bool IsTyping { get; private set; }

        private DialogueVoicePlayback _voicePlayback;
        private SpeakerProfileSO _currentSpeaker;
        private int _visibleCharacters;
        private float _nextCharacterTime;

        public void Initialize(AudioSource voiceSource)
        {
            _voicePlayback = new DialogueVoicePlayback(voiceSource);
        }

        public void StartTyping(string text, SpeakerProfileSO speaker, bool animate)
        {
            Stop();
            _currentSpeaker = speaker;

            // Passamos o perfil inteiro (SpeakerProfileSO) para o Playback
            if (_currentSpeaker != null && _currentSpeaker.voiceSound != null)
            {
                _voicePlayback?.Prepare(_currentSpeaker);
            }

            _textComponent.text = text ?? string.Empty;
            _textComponent.maxVisibleCharacters = int.MaxValue;
            _textComponent.ForceMeshUpdate();

            int totalChars = _textComponent.textInfo.characterCount;
            _visibleCharacters = 0;
            IsTyping = animate && totalChars > 0;
            _textComponent.maxVisibleCharacters = IsTyping ? 0 : int.MaxValue;
            _nextCharacterTime = Time.unscaledTime;
        }

        public void CompleteImmediately()
        {
            Stop();
            if (_textComponent != null)
            {
                _textComponent.maxVisibleCharacters = int.MaxValue;
            }
        }

        public void Stop()
        {
            IsTyping = false;
        }

        private void Update()
        {
            if (!IsTyping || _textComponent == null) return;

            int count = _textComponent.textInfo.characterCount;
            bool shouldPlayVoice = false;

            while (_visibleCharacters < count && Time.unscaledTime >= _nextCharacterTime)
            {
                char c = _textComponent.textInfo.characterInfo[_visibleCharacters].character;
                _visibleCharacters++;

                if (char.IsWhiteSpace(c)) continue;

                shouldPlayVoice |= char.IsLetterOrDigit(c);
                _nextCharacterTime += 1f / Mathf.Max(1f, _charactersPerSecond);

                bool isLastChar = _visibleCharacters == count;
                if (char.IsPunctuation(c) && (isLastChar || !char.IsPunctuation(_textComponent.textInfo.characterInfo[_visibleCharacters].character)))
                {
                    _nextCharacterTime += _punctuationDelay;
                }
            }

            _textComponent.maxVisibleCharacters = _visibleCharacters;

            // Passamos o perfil inteiro (SpeakerProfileSO) na hora de tocar
            if (shouldPlayVoice && _currentSpeaker != null && _currentSpeaker.voiceSound != null)
            {
                _voicePlayback?.TryPlay(_currentSpeaker, _voiceVolume);
            }

            if (_visibleCharacters >= count)
            {
                IsTyping = false;
            }
        }

        private void OnDestroy()
        {
            _voicePlayback?.Dispose();
        }
    }
}