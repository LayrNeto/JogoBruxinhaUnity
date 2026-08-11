using UnityEngine;

public class CauldronPOVManager : MonoBehaviour
{
    public CauldronController cauldronController;

    private void Start()
    {
        GameStateManager.Instance.inputControls.UI.Close.performed += ctx => ClosePOV();
    }

    private void OnDestroy()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.inputControls.UI.Close.performed -= ctx => ClosePOV();
        }
    }

    public void ClosePOV()
    {
        if (FadeManager.Instance.isFading) return;

        FadeManager.Instance.StartFullFade(0f, 0.4f, () =>
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.ForceCloseDialogue();
            }

            if (cauldronController != null)
            {
                cauldronController.onValidateIngredientDrop = null;
                cauldronController.onPotionFinished = null;
                cauldronController.onIngredientAdded = null;
                cauldronController.onValidateCauldronClick = null;
            }

            gameObject.SetActive(false);
        });
    }
}