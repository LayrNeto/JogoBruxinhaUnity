using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CreditsManager : MonoBehaviour
{
    [Header("References")]
    public Image contentImage;
    public GameObject nextButton;
    public GameObject previousButton;

    [Header("Screens")]
    public Sprite[] screens;

    private int currentPage = 0;

    void OnEnable()
    {
        GameStateManager.Instance.inputControls.UI.Close.performed += OnCloseCredits;
    }

    void OnDisable()
    {
        GameStateManager.Instance.inputControls.UI.Close.performed -= OnCloseCredits;        
    }
    
    private void OnCloseCredits(InputAction.CallbackContext ctx)
    {
        CloseCredits();
    }
    void Start()
    {
        UpdatePage();
    }

    public void NextPage()
    {
        if (currentPage < screens.Length)
        {
            currentPage++;
            UpdatePage();
        }
    }

    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdatePage();
        }
    }

    private void UpdatePage()
    {
        contentImage.sprite = screens[currentPage];
        nextButton.SetActive(currentPage < (screens.Length - 1));
        previousButton.SetActive(currentPage > 0);
    }

    public void CloseCredits()
    {
        currentPage = 0;
        UpdatePage();
        gameObject.SetActive(false);
    }
}
