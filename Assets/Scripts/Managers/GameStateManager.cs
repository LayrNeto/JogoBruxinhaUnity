using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    
    public enum GameState {MENU, TOP_DOWN, INVENTORY, CUTSCENE, POV, PAUSE};

    private Stack<GameState> stateStack = new Stack<GameState>();

    public PlayerControls inputControls;

    public void Init()
    {
        if (Instance == null)
        {
            Instance = this;
            
            inputControls = new PlayerControls();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        inputControls?.Dispose();
    }

    void Start()
    {
        GameState state = (SceneManager.GetActiveScene().name == "MainMenu") ? GameState.MENU : GameState.TOP_DOWN;
        stateStack.Push(state);
        ApplyCurrentState();
    }

    private void OnEnable()
    {
        if (inputControls != null)
        {
            inputControls.Global.Enable();
        }
    }

    private void OnDisable()
    {
        if (inputControls != null)
        {
            inputControls.Global.Disable();
        }
    }

    public void PushState(GameState newState)
    {
        stateStack.Push(newState);
        ApplyCurrentState();
    }

    public void PopState()
    {
        if (stateStack.Count > 1) 
        {
            stateStack.Pop(); 
            ApplyCurrentState();
        }
    }

    public void ResetToDefaultState()
    {
        stateStack.Clear();
        PushState(GameState.MENU);
    }

    private void ApplyCurrentState()
    {
        GameState currentState = stateStack.Peek();
        Debug.Log($"Current State: {currentState}");

        if (currentState == GameState.TOP_DOWN)
        {
            Time.timeScale = 1f;
            
            inputControls.UI.Disable();
            inputControls.Player.Enable(); 
            inputControls.Global.Enable();
        }
        else if (currentState == GameState.CUTSCENE)
        {
            Time.timeScale = 1f;

            inputControls.Player.Disable();
            inputControls.Global.Disable();
        }
        else if (currentState == GameState.POV)
        {
            Time.timeScale = 1f;
            
            inputControls.Player.Disable();
            inputControls.UI.Enable();
            inputControls.Global.Disable();
        }
        else if (currentState == GameState.MENU)
        {
            Time.timeScale = 1f;
            inputControls.Player.Disable();
            inputControls.UI.Enable();
            inputControls.Global.Disable();
        }
        else if (currentState == GameState.INVENTORY)
        {
            Time.timeScale = 0f;
            
            inputControls.Player.Disable();
            inputControls.UI.Enable();
        }
        else if (currentState == GameState.PAUSE)
        {
            Time.timeScale = 0f;

            inputControls.Player.Disable();
            inputControls.UI.Enable();
            inputControls.Global.Disable();
        }
    }
}
