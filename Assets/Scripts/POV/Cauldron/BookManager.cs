using UnityEngine;
using UnityEngine.UI;

public class BookManager : MonoBehaviour
{
    [Header("References")]
    public Image contentImage;
    public GameObject tableSupport;
    public GameObject nextButton;
    public GameObject previousButton;
    public GameObject ritualSymbol;

    [Header("Audio")]
    public SoundDataSO turningPageSound;
    public SoundDataSO openBookSound;

    [Header("Pages")]
    public Sprite[] pages;

    private int currentPage = 0;

    void Start()
    {
        UpdatePage();
    }

    public void NextPage()
    {
        if (currentPage < pages.Length)
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
        contentImage.sprite = pages[currentPage];
        nextButton.SetActive(currentPage < (pages.Length - 1));
        previousButton.SetActive(currentPage > 0);
        ritualSymbol.SetActive(currentPage == (pages.Length -1));
        AudioManager.Instance.PlaySFX(turningPageSound);
    }

    private void OpenBook()
    {
        AudioManager.Instance.PlaySFX(openBookSound);
        tableSupport.SetActive(true);
    }
}
