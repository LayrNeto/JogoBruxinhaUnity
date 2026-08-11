using UnityEngine;

public class SystemBootstrapper : MonoBehaviour
{
    [Header("Managers")]
    public SaveManager saveManager;
    public GameStateManager gameStateManager;
    public FadeManager fadeManager;
    public AudioManager audioManager;

    private void Awake()
    {
        saveManager.Init();
        gameStateManager.Init();
        fadeManager.Init();
        audioManager.Init();
    }
}