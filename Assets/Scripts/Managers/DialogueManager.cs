using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject dialogueCanvasRoot; 
    public GameObject nextButton;
    public Image dialogueBackgroundImage; 
    public TextMeshProUGUI dialogueText;      

    [Header("Assets")]
    public Sprite playerDialogueBox;
    private Sprite currentNPCBox;

    [SerializeField]
    private List<DialogueLine> currentDialogueList;
    private int currentLineIndex = 0;
    private Action dynamicOnFinishCallback;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        GameStateManager.Instance.inputControls.UI.SkipDialogue.performed += OnSkipDialoguePerformed;
    }

    private void OnDestroy()
    {
        if (GameStateManager.Instance != null && GameStateManager.Instance.inputControls != null)
        {
            GameStateManager.Instance.inputControls.UI.SkipDialogue.performed -= OnSkipDialoguePerformed;
        }
    }

    private void OnSkipDialoguePerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        ShowNextLine();
    }
    
    public void PlayDialogue(Sprite boxSprite, List<DialogueLine> dialogue, Action onFinish = null)
    {
        currentDialogueList = dialogue;
        currentNPCBox = boxSprite;
        currentLineIndex = 0;
        dynamicOnFinishCallback = onFinish;
        
        if (dialogueCanvasRoot) dialogueCanvasRoot.SetActive(true);
        if (nextButton) nextButton.SetActive(true);

        dialogueBackgroundImage.gameObject.SetActive(true);
        
        ShowNextLine();
    }

    public void ShowNextLine()
    {
        if (currentLineIndex < currentDialogueList.Count)
        {
            DialogueLine currentLine = currentDialogueList[currentLineIndex];
            dialogueText.text = currentLine.text;
            dialogueBackgroundImage.sprite = (currentLine.speaker == SpeakerType.Player) ? playerDialogueBox : currentNPCBox;
            currentLineIndex++;
        }
        else
        {
            FinishDialogue();
        }
    }

    private void FinishDialogue()
    {
        if (dialogueCanvasRoot) dialogueCanvasRoot.SetActive(false);
        if (nextButton) nextButton.SetActive(false);

        dialogueBackgroundImage.gameObject.SetActive(false);
            
        dynamicOnFinishCallback?.Invoke();
        dynamicOnFinishCallback = null;
    }

    public void ForceCloseDialogue()
    {
        if (dialogueCanvasRoot != null) dialogueCanvasRoot.SetActive(false);
        dialogueBackgroundImage.gameObject.SetActive(false);
        
        dynamicOnFinishCallback = null; 
    }

    public void SetSortOrder(int newOrder)
    {
        if (dialogueCanvasRoot != null)
        {
            Canvas canvas = dialogueCanvasRoot.GetComponent<Canvas>();
            if (canvas != null) canvas.sortingOrder = newOrder;
        }
    }
}