using JogoBruxinha.Gameplay.GameFlow;
using UnityEngine;

namespace JogoBruxinha.Gameplay.UI
{
    public sealed class CanvasStateManager : MonoBehaviour
    {
        [Header("State Settings")]
        [Tooltip("Qual estado este canvas deve forçar ao ser ativado?")]
        [SerializeField] private GameStateManager.GameState _stateToPush = GameStateManager.GameState.POV;

        public GameStateManager.GameState StateToPush => _stateToPush;

        private void OnEnable()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.PushState(_stateToPush);
            }
        }

        private void OnDisable()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.PopState();
            }
        }
    }
}