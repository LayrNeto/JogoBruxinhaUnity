using UnityEngine;

public class CanvasStateManager : MonoBehaviour
{
    [Header("State Settings")]
    [Tooltip("Which state should this canvas force when opened?")]
    public GameStateManager.GameState stateToPush = GameStateManager.GameState.POV;

    private void OnEnable()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.PushState(stateToPush);
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