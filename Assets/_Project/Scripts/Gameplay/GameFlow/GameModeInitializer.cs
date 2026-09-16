using UnityEngine;

namespace JogoBruxinha.Gameplay.GameFlow
{
    public sealed class GameModeInitializer : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private SessionDataSO _sessionData;

        [Header("Modes")]
        [SerializeField] private GameObject _normalModeRoot;
        [SerializeField] private GameObject _tutorialModeRoot;

        private void Awake()
        {
            Debug.Assert(_sessionData != null, $"Missing _sessionData reference on {name}", this);
            Debug.Assert(_normalModeRoot != null, $"Missing _normalModeRoot reference on {name}", this);
            Debug.Assert(_tutorialModeRoot != null, $"Missing _tutorialModeRoot reference on {name}", this);

            bool isTutorial = _sessionData != null && _sessionData.tutorialData != null;

            if (_normalModeRoot != null)
            {
                _normalModeRoot.SetActive(!isTutorial);
            }

            if (_tutorialModeRoot != null)
            {
                _tutorialModeRoot.SetActive(isTutorial);
            }
        }
    }
}