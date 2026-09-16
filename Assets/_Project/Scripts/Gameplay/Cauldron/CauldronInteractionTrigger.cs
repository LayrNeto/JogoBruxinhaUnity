using JogoBruxinha.Core.SceneManagement;
using JogoBruxinha.Gameplay.GameFlow;
using JogoBruxinha.Gameplay.Tutorial;
using UnityEngine;

namespace JogoBruxinha.Gameplay.Cauldron
{
    public sealed class CauldronInteractionTrigger : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private SessionDataSO _sessionData;

        [Header("UI References")]
        [SerializeField] private GameObject _cauldronCanvasRoot;

        [Header("Managers")]
        [SerializeField] private TutorialManager _tutorialManager;

        private void Awake()
        {
            Debug.Assert(_sessionData != null, $"Missing _sessionData reference on {name}", this);
            Debug.Assert(_cauldronCanvasRoot != null, $"Missing _cauldronCanvasRoot reference on {name}", this);
        }

        public void OpenCauldron()
        {
            if (FadeManager.Instance != null && FadeManager.Instance.IsFading) return;

            if (FadeManager.Instance == null)
            {
                _cauldronCanvasRoot.SetActive(true);
                return;
            }

            FadeManager.Instance.StartFullFade(0f, 1f, () =>
            {
                _cauldronCanvasRoot.SetActive(true);

                if (_sessionData.tutorialData != null)
                {
                    if (_tutorialManager != null)
                    {
                        _tutorialManager.InteractWithTutorialCauldron();
                    }
                    else
                    {
                        Debug.LogWarning($"Tutorial ativo, mas _tutorialManager não referenciado em {name}!", this);
                    }
                }
            });
        }
    }
}