using JogoBruxinha.Core.SceneManagement;
using JogoBruxinha.Gameplay.Dialogue;
using JogoBruxinha.Gameplay.GameFlow;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JogoBruxinha.Gameplay.Cauldron
{
    public sealed class CauldronPOVManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CauldronController _cauldronController;

        private void Awake()
        {
            Debug.Assert(_cauldronController != null, $"Missing _cauldronController reference on {name}", this);
        }

        private void Start()
        {
            if (GameStateManager.Instance != null && GameStateManager.Instance.InputControls != null)
            {
                GameStateManager.Instance.InputControls.UI.Close.performed += OnCloseInput;
            }
        }

        private void OnDestroy()
        {
            if (GameStateManager.Instance != null && GameStateManager.Instance.InputControls != null)
            {
                GameStateManager.Instance.InputControls.UI.Close.performed -= OnCloseInput;
            }
        }

        private void OnCloseInput(InputAction.CallbackContext context)
        {
            ClosePOV();
        }

        public void ClosePOV()
        {
            if (FadeManager.Instance != null && FadeManager.Instance.IsFading) return;

            if (FadeManager.Instance == null)
            {
                ExecuteCloseActions();
                return;
            }

            FadeManager.Instance.StartFullFade(0f, 0.4f, ExecuteCloseActions);
        }

        private void ExecuteCloseActions()
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.ForceCloseDialogue();
            }

            if (_cauldronController != null)
            {
                _cauldronController.onValidateIngredientDrop = null;
                _cauldronController.onPotionFinished = null;
                _cauldronController.onIngredientAdded = null;
                _cauldronController.onValidateCauldronClick = null;
            }

            gameObject.SetActive(false);
        }
    }
}