using UnityEngine;

public class GameBootstrapper : MonoBehaviour
{
    [Header("Data")]
    public SessionDataSO sessionData;

    [Header("Modes")]
    public GameObject normalModeRoot;
    public GameObject tutorialModeRoot;

    private void Awake()
    {
        if (sessionData != null && sessionData.tutorialData != null)
        {
            normalModeRoot.SetActive(false);
            tutorialModeRoot.SetActive(true);
        }
        else
        {
            normalModeRoot.SetActive(true);
            tutorialModeRoot.SetActive(false);
        }
    }
}