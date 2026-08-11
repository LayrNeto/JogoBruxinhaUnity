using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject pauseCanvas;
    public GameObject savingScreen;

   void Start()
    {
        GameStateManager.Instance.inputControls.Player.Pause.performed += OnPausePerformed;
        GameStateManager.Instance.inputControls.UI.Close.performed += OnClosePerformed;
    }

    void OnDestroy()
    {
        if (GameStateManager.Instance != null && GameStateManager.Instance.inputControls != null)
        {
            GameStateManager.Instance.inputControls.Player.Pause.performed -= OnPausePerformed;        
            GameStateManager.Instance.inputControls.UI.Close.performed -= OnClosePerformed;        
        }
    }


    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        Pause();
    }

    private void OnClosePerformed(InputAction.CallbackContext ctx)
    {
        Resume();
    }

    public void Resume()
    {
        if (!pauseCanvas.activeSelf) return;

        pauseCanvas.SetActive(false);
        GameStateManager.Instance.PopState();
    }

    public void Quit()
    {
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void Menu()
    {
        GameStateManager.Instance.ResetToDefaultState();

        FadeManager.Instance.StartTransition("MainMenu", "", 0.2f, 0.2f);
    }

    public void Pause()
    {
        pauseCanvas.SetActive(true);

        GameStateManager.Instance.PushState(GameStateManager.GameState.PAUSE);
    }

    public void Save()
    {
        StartCoroutine(SaveRoutine());
    }

    private IEnumerator SaveRoutine()
    {
        savingScreen.SetActive(true);

        yield return new WaitForSecondsRealtime(2.4f);
        SaveManager.Instance.SaveGame();   

        savingScreen.SetActive(false); 
    }
}
