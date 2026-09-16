using System.Collections.Generic;
using JogoBruxinha.Core.Input;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JogoBruxinha.Gameplay.GameFlow
{
    [DefaultExecutionOrder(-90)]
    public sealed class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        public enum GameState { MENU, TOP_DOWN, INVENTORY, CUTSCENE, POV, PAUSE }

        private readonly Stack<GameState> _stateStack = new Stack<GameState>();

        public PlayerControls InputControls { get; private set; }
        public GameState CurrentState => _stateStack.Count > 0 ? _stateStack.Peek() : GameState.MENU;

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InputControls = new PlayerControls();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                InputControls?.Dispose();
                Instance = null;
            }
        }

        private void Start()
        {
            GameState initialState = (SceneManager.GetActiveScene().name == "MainMenu") ? GameState.MENU : GameState.TOP_DOWN;
            _stateStack.Push(initialState);
            ApplyCurrentState();
        }

        private void OnEnable()
        {
            InputControls?.Global.Enable();
        }

        private void OnDisable()
        {
            InputControls?.Global.Disable();
        }

        public void PushState(GameState newState)
        {
            _stateStack.Push(newState);
            ApplyCurrentState();
        }

        public void PopState()
        {
            if (_stateStack.Count > 1)
            {
                _stateStack.Pop();
                ApplyCurrentState();
            }
        }

        public void ResetToDefaultState()
        {
            _stateStack.Clear();
            PushState(GameState.MENU);
        }

        private void ApplyCurrentState()
        {
            if (InputControls == null || _stateStack.Count == 0) return;

            GameState currentState = _stateStack.Peek();
            Debug.Log($"Current State: {currentState}");

            switch (currentState)
            {
                case GameState.TOP_DOWN:
                    Time.timeScale = 1f;
                    InputControls.UI.Disable();
                    InputControls.Player.Enable();
                    InputControls.Global.Enable();
                    break;

                case GameState.CUTSCENE:
                    Time.timeScale = 1f;
                    InputControls.UI.Enable();
                    InputControls.Player.Disable();
                    InputControls.Global.Disable();
                    break;

                case GameState.POV:
                case GameState.MENU:
                    Time.timeScale = 1f;
                    InputControls.Player.Disable();
                    InputControls.UI.Enable();
                    InputControls.Global.Disable();
                    break;

                case GameState.INVENTORY:
                    Time.timeScale = 0f;
                    InputControls.Player.Disable();
                    InputControls.UI.Enable();
                    break;

                case GameState.PAUSE:
                    Time.timeScale = 0f;
                    InputControls.Player.Disable();
                    InputControls.UI.Enable();
                    InputControls.Global.Disable();
                    break;
            }
        }
    }
}