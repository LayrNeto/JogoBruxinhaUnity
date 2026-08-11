using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using Unity.Cinemachine;

public class TutorialManager : MonoBehaviour
{
    [Header("Data")]
    public SessionDataSO sessionData;
    public InventoryDataSO inventoryData;

    [Header("UI References")]
    public Image npcImage;

    [Header("Cauldron References")]
    public CauldronController cauldronController;
    public Button spellButton;
    public Button exitButton;

    [Header("Counter References")]
    public PatientDeliveryManager patientDelivery;

    [Header("Cutscene References (Step 5)")]
    public Transform agataTopDownTransform; 
    public SpriteRenderer agataTopDownSpriteRenderer;
    public Sprite agataCatSprite;
    public GameObject smokePrefab;
    public PlayerController playerController;
    public CinemachineCamera vcam; 


    [Header("Tutorial Logic")]
    [Tooltip("Cada índice do array representa um Step do tutorial (0, 1, 2...)")]
    public UnityEvent[] tutorialSteps;
    
    private Transform originalCameraTarget;

    private void Awake()
    {   
        ApplyCurrentStep();
    }

    private void Start()
    {
        if (sessionData.tutorialData != null)
        {
            EvaluateSceneLoadState();
        }
    }

    private void EvaluateSceneLoadState()
    {
        int step = sessionData.tutorialData.tutorialStep;

        if (step == 1)
        {
            if (CollectedAllIngredients())
            {
                Debug.Log("Jogador voltou da estufa com tudos os ingredientes! Avançando para Step 2.");
                AdvanceTutorial(); 
            }
            else
            {
                Debug.Log("Jogador voltou da estufa, mas faltam ingredientes. Continua no Step 1.");
            }
        }
        if (step == 5)
        {
            StartCoroutine(TransformationRoutine());
        }
    }

    private bool CollectedAllIngredients()
    {
        TutorialDataSO tData = sessionData.tutorialData;

        if (tData.requiredIngredients == null || tData.requiredIngredients.Count == 0) return true;

        foreach (var plant in tData.requiredIngredients)
        {
            if (!inventoryData.savedInv.ContainsKey(plant) || inventoryData.savedInv[plant] < 1)
            {
                return false;
            }
        }
        return true;
    }

    public void AdvanceTutorial()
    {
        sessionData.tutorialData.tutorialStep++;
        ApplyCurrentStep();
    }

    private void ApplyCurrentStep()
    {
        int currentStep = sessionData.tutorialData.tutorialStep;

        if (currentStep < tutorialSteps.Length)
        {
            tutorialSteps[currentStep]?.Invoke();
        }
        else
        {
            Debug.Log("Tutorial Finalizado! Iniciando transição...");

            sessionData.tutorialData = null;
            sessionData.ResetSession();
            inventoryData.ClearData();

            GameStateManager.Instance.PopState();

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            FadeManager.Instance.StartTransition(currentScene, "BedSpawn", 0.5f, 1.5f);
        }
    }

    public void InteractWithTutorialCounter()
    {
        int step = sessionData.tutorialData.tutorialStep;

        if (step == 3)
        {
            if (sessionData.potionAwaitingDelivery && inventoryData.craftedPotions.Count > 0)
            {
                patientDelivery.potionButton.gameObject.SetActive(true);
                patientDelivery.potionButton.enabled = true;
                patientDelivery.potionImage.sprite = inventoryData.craftedPotions[0].potionData.tableSprite;

                patientDelivery.onPotionDelivery = DeliverTutorialPotion;
            }
        }

        TutorialContext currentContext = EvaluateCounterContext();
        PlayTutorialDialogue(currentContext, true); 
    }

    private void DeliverTutorialPotion()
    {
        patientDelivery.onPotionDelivery = null;

        patientDelivery.potionButton.gameObject.SetActive(false);
        sessionData.potionAwaitingDelivery = false;
        inventoryData.craftedPotions.Clear();

        AdvanceTutorial();

        Debug.Log("Tocando diálogo de entrega de poção, avançando para o step 4");
        PlayTutorialDialogue(TutorialContext.BALCAO_STEP_3, true);
    }

    public void InteractWithTutorialCauldron()
    {
        int step = sessionData.tutorialData.tutorialStep;
        
        if (spellButton)
        {
            if (step == 2) spellButton.enabled = false; 
            if (step == 4) spellButton.enabled = true; 
        }

        if (cauldronController)
        {
            if (step == 2) cauldronController.onValidateIngredientDrop = ValidateTutorialIngredient;

            cauldronController.onValidateCauldronClick = ValidateCauldronClick;
            cauldronController.onIngredientAdded = EvaluateIngredientCount;
            cauldronController.onPotionFinished = EvaluatePotionFinished;
        }

        TutorialContext currentContext = EvaluateCauldronContext();
        PlayTutorialDialogue(currentContext, false); 
    }

    private bool ValidateTutorialIngredient(PlantDataSO droppedPlant)
    {
        if (!sessionData.tutorialData.correctRecipe.Contains(droppedPlant))
        {
            Debug.Log("Jogador tentou colocar a planta errada no Tutorial!");

            PlayTutorialDialogue(TutorialContext.CALDEIRAO_ERRO_RECEITA, false);
            
            return false;
        }


        return true;
    }

    private void EvaluateIngredientCount(int currentCount)
    {
        int step = sessionData.tutorialData.tutorialStep;
        
        if (step == 4 && currentCount == 3)
        {
            PlayTutorialDialogue(TutorialContext.CALDEIRAO_PRE_POCAO_STEP_4, false);
        }
    }

    private bool ValidateCauldronClick()
    {
        int step = sessionData.tutorialData.tutorialStep;

        if (step == 4)
        {
            PlayTutorialDialogue(TutorialContext.CALDEIRAO_ERRO_CLIQUE, false);
            return false; 
        }

        return true;
    }

    private void EvaluatePotionFinished(bool wasSpellUsed)
    {
        int step = sessionData.tutorialData.tutorialStep;

        if (step == 2)
        {
            Debug.Log("Poção normal feita com sucesso! Avançando pro Step 3.");
            PlayTutorialDialogue(TutorialContext.CALDEIRAO_FIM_STEP_2, false);
            AdvanceTutorial();
        }
        else if (step == 4)
        {
            Debug.Log("Poção mágica feita! Preparando Cutscene.");
            PlayTutorialDialogue(TutorialContext.CALDEIRAO_FIM_STEP_4, false);
            exitButton.gameObject.SetActive(false);
        }
    }

    private void PlayTutorialDialogue(TutorialContext context, bool showCharacterSprite)
    {
        Sprite agataSprite = showCharacterSprite ? sessionData.tutorialData.agataPovSprite : null;
        if (agataSprite != null)
        {
            npcImage.gameObject.SetActive(true);
            npcImage.sprite = agataSprite;
            npcImage.SetNativeSize();
        }
        else
        {
            npcImage.gameObject.SetActive(false);
        }
        
        DialogueSequence seq = sessionData.tutorialData.GetDialogue(context);
        if (seq.lines == null || seq.lines.Count == 0) return; 

        Sprite agataBox = sessionData.tutorialData.agataDialogueBox;
        Action onDialogueClosed = () => EvaluateTutorialAdvance(context);

        DialogueManager.Instance.PlayDialogue(agataBox, seq.lines, onDialogueClosed);
    }

    private void EvaluateTutorialAdvance(TutorialContext contextPlayed)
    {
        if (contextPlayed == TutorialContext.BALCAO_STEP_0)
        {
            Debug.Log("Terminou o diálogo inicial! Avançando para o Step 1.");
            AdvanceTutorial(); 
        }
        if (contextPlayed == TutorialContext.CALDEIRAO_FIM_STEP_4)
        {
            AdvanceTutorial();
            exitButton.onClick.Invoke();
            StartTransformationCutscene();
        }
        if (contextPlayed == TutorialContext.POS_CUTSCENE_STEP_5)
        {
            Debug.Log("Terminou o último diálofo, finalizando o tutorial");
            DialogueManager.Instance.SetSortOrder(100);
            AdvanceTutorial();
        }

        Debug.Log($"Avaliando o que fazer no fim do diálogo para o contexto {contextPlayed}");
    }

    private TutorialContext EvaluateCounterContext()
    {
        int step = sessionData.tutorialData.tutorialStep;

        if (step == 0) return TutorialContext.BALCAO_STEP_0;    
        if (step == 1) return TutorialContext.BALCAO_STEP_1;

        return TutorialContext.DEFAULT;
    }

    private TutorialContext EvaluateCauldronContext()
    {
        int step = sessionData.tutorialData.tutorialStep;

        if (step == 2) return TutorialContext.CALDEIRAO_INICIO_STEP_2;
        if (step == 4) return TutorialContext.CALDEIRAO_INICIO_STEP_4; 

        return TutorialContext.DEFAULT;
    }

    public void StartTransformationCutscene()
    {
        StartCoroutine(TransformationRoutine());
    }

    private IEnumerator TransformationRoutine()
    {
        GameStateManager.Instance.PushState(GameStateManager.GameState.CUTSCENE);
        playerController.ChangeIdleDirection(Vector2.left);

        if (vcam)
        {
            originalCameraTarget = vcam.Follow;
            vcam.Follow = agataTopDownTransform;
        }

        float shakeDuration = 2f;
        float shakeIntensity = 0.1f;
        Vector3 originalPos = agataTopDownTransform.position;
        float timer = 0f;

        while (timer < shakeDuration)
        {
            timer += Time.deltaTime;
            float offsetX = UnityEngine.Random.Range(-1f, 1f) * shakeIntensity;
            float offsetY = UnityEngine.Random.Range(-1f, 1f) * shakeIntensity;
            
            agataTopDownTransform.position = originalPos + new Vector3(offsetX, offsetY, 0);
            
            yield return null; 
        }
        
        agataTopDownTransform.position = originalPos;

        if (smokePrefab)
        {
            Instantiate(smokePrefab, agataTopDownTransform.position + new Vector3(0, -1.2f, 0), Quaternion.identity);
        }

        if (agataTopDownSpriteRenderer)
        {
            agataTopDownSpriteRenderer.sprite = agataCatSprite;
            agataTopDownTransform.position = originalPos + new Vector3(0, 0f, 0);
        }

        yield return new WaitForSeconds(1.5f);

        FadeManager.Instance.StartFadeOut(1.5f, () =>
        {
            vcam.Follow = originalCameraTarget;
            DialogueManager.Instance.SetSortOrder(1001);
            PlayTutorialDialogue(TutorialContext.POS_CUTSCENE_STEP_5, false);
        });
    }
}