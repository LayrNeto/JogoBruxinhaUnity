using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject tutorialCanvas;
    public Button continueButton;

    [Header("Data")]
    public SessionDataSO sessionData;
    public TutorialDataSO tData;
    public InventoryDataSO inventoryData;

    [Header("Audio")]
    public SoundDataSO mainMenuMusic;

    void Start()
    {
        if (continueButton) continueButton.interactable = SaveManager.Instance.HasSaveFile();
        if (mainMenuMusic) AudioManager.Instance.PlayMusic(mainMenuMusic);
    }

    void OnEnable()
    {
        GameStateManager.Instance.inputControls.UI.Close.performed += OnCloseTutorialCanvas;
    }

    void OnDisable()
    {
        GameStateManager.Instance.inputControls.UI.Close.performed -= OnCloseTutorialCanvas;        
    }

    private void OnCloseTutorialCanvas(InputAction.CallbackContext ctx)
    {
        CloseTutorialCanvas();
    }

    private void CloseTutorialCanvas()
    {
        tutorialCanvas.SetActive(false);
    }

    public void StartTutorial()
    {
        tData.ResetTutorial();
        sessionData.ResetSession();
        inventoryData.ClearData();

        sessionData.tutorialData = tData;

        GameStateManager.Instance.PopState();
        GameStateManager.Instance.PushState(GameStateManager.GameState.TOP_DOWN);

        FadeManager.Instance.StartTransition("HouseScene", "TutorialSpawn", 1f, 1f);
    }

    public void SkipTutorial()
    {
        tData.ResetTutorial();
        sessionData.ResetSession();
        inventoryData.ClearData();

        sessionData.tutorialData = null;

        GameStateManager.Instance.PopState();
        GameStateManager.Instance.PushState(GameStateManager.GameState.TOP_DOWN);

        FadeManager.Instance.StartTransition("HouseScene", "BedSpawn", 1f, 1f);
    }

    public void Quit()
    {
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void ContinueGame()
    {
        SaveManager.Instance.LoadGame();

        GameStateManager.Instance.PopState();
        GameStateManager.Instance.PushState(GameStateManager.GameState.TOP_DOWN);

        StartCoroutine(ContinueGameRoutine());
    }

    private IEnumerator ContinueGameRoutine()
    {
        yield return new WaitForSeconds(2.5f);

        string savedScene = SaveManager.Instance.sessionData.currentScene;
        FadeManager.Instance.StartTransition(savedScene, "SAVED_SPAWN", 1f, 1f);
    }
}
