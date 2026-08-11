using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Interactable))]
public class WorldPlant : MonoBehaviour
{
    [Header("Plant Data")]
    public PlantDataSO plantData;
    public string uniqueInstanceID; 

    [Header("Dependencies")]
    public SessionDataSO sessionData; 

    [Header("Collect Event")]
    public GameEventItemStruct collectEvent;

    private Interactable interactableComponent;
    private SpriteRenderer sr;

    private int visualIndex; 
    private int soundIndex;
    private bool isCollected = false;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        interactableComponent = GetComponent<Interactable>();

        if (string.IsNullOrEmpty(uniqueInstanceID))
        {
            Debug.LogError($"Planta {gameObject.name} está sem uniqueInstanceID!");
            return;
        }

        if (sessionData.plantDaysRemaining.ContainsKey(uniqueInstanceID) && 
            sessionData.plantDaysRemaining[uniqueInstanceID] > 0)
        {
            SetupCollectedPlant();
        }
        else
        {
            SetupAlivePlant();
        }
    }

    private void SetupAlivePlant()
    {
        isCollected = false;

        if (interactableComponent) interactableComponent.canInteract = true;

        if (plantData.collectSound.Length > 0)
        {
            soundIndex = Random.Range(0, plantData.collectSound.Length);
        }

        if (plantData.worldSprites.Length > 0)
        {
            visualIndex = Random.Range(0, plantData.worldSprites.Length);
            sr.sprite = plantData.worldSprites[visualIndex];
        }
    }

    private void SetupCollectedPlant()
    {
        isCollected = true;

        if (interactableComponent) interactableComponent.canInteract = false;

        if (plantData.collectedSprites.Length > visualIndex)
        {
            sr.sprite = plantData.collectedSprites[visualIndex];
        }
    }

    public void Collect()
    {
        if (isCollected) return;

        isCollected = true;
        if (interactableComponent) interactableComponent.canInteract = false;

        if (plantData.collectedSprites.Length > visualIndex)
        {
            sr.sprite = plantData.collectedSprites[visualIndex];
        }
        
        sessionData.SetPlantGrowth(uniqueInstanceID, plantData.daysToGrow);

        collectEvent.Raise(new ItemStruct(plantData, 1));

        AudioManager.Instance.PlaySFX(plantData.collectSound[soundIndex]);
    }
}