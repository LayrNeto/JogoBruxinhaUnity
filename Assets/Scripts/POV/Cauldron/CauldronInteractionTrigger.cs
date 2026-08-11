using UnityEngine;

public class CauldronInteractionTrigger : MonoBehaviour
{
    [Header("Dependencies")]
    public SessionDataSO sessionData;
    
    [Header("UI References")]
    public GameObject cauldronCanvasRoot; 
    
    [Header("Managers")]
    public TutorialManager tutorialManager; 

    public void OpenCauldron()
    {
        if (FadeManager.Instance.isFading) return;

        FadeManager.Instance.StartFullFade(0f, 1f, () =>
        {
            cauldronCanvasRoot.SetActive(true);

            if (sessionData.tutorialData != null)
            {
                if (tutorialManager != null)
                {
                    tutorialManager.InteractWithTutorialCauldron();
                }
                else
                {
                    Debug.LogWarning("Tutorial ativo, mas TutorialManager não referenciado no gatilho do Caldeirão!");
                }
            }
        });
    }
}