using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    [Header("Scriptable Objects")]
    public SessionDataSO sessionData;
    public TutorialDataSO tutorialData;
    public InventoryDataSO inventoryData;

    [Header("Dependencies")]
    public EntityTrackerSO tracker;

    [Header("All Itens Database")]
    public List<ItemDataSO> allItemsDatabase; 

    public static SaveManager Instance { get; private set; }

    private string SavePath => Application.persistentDataPath + "/slot_01.json";

    public void Init()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveGame()
    {
        sessionData.currentScene = SceneManager.GetActiveScene().name;
        if (tracker.player != null)
        {
            sessionData.playerPos = tracker.player.transform.position;
        }
        if (tracker.companion != null)
        {
            sessionData.companionPos = tracker.companion.transform.position;
        }
        
        GameSaveData data = new GameSaveData();

        // Session Data
        data.currentScene = sessionData.currentScene;
        data.playerPos = new SaveVector3(sessionData.playerPos);
        data.companionPos = new SaveVector3(sessionData.companionPos);
        data.currentDay = sessionData.currentDay;
        data.isNight = sessionData.isNight;
        data.potionAwaitingDelivery = sessionData.potionAwaitingDelivery;
        data.savedNPCState = (int)sessionData.savedNPCState;
        data.hasNPCSpawnedToday = sessionData.hasNPCSpawnedToday;
        data.interactionCount = sessionData.interactionCount;
        data.plantDaysRemaining = new Dictionary<string, int>(sessionData.plantDaysRemaining);

        // Tutorial Data
        data.isTutorialActive = sessionData.tutorialData != null;
        if (data.isTutorialActive)
        {
            data.tutorialStep = tutorialData.tutorialStep;
        }

        // Inventory Data
        foreach (KeyValuePair<ItemDataSO, int> pair in inventoryData.savedInv)
        {
            data.savedInv.Add(pair.Key.name, pair.Value); 
        }
        data.craftedPotions = new List<BrewedPotion>(inventoryData.craftedPotions);

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(SavePath, json);
        
        Debug.Log("Jogo salvo com sucesso em: " + SavePath);
    }

    public void LoadGame()
    {
        if (!File.Exists(SavePath)) return;

        string json = File.ReadAllText(SavePath);
        GameSaveData data = JsonConvert.DeserializeObject<GameSaveData>(json);

        // Session Data
        sessionData.currentScene = data.currentScene;
        sessionData.playerPos = data.playerPos.ToUnityVector();
        sessionData.companionPos = data.companionPos.ToUnityVector();
        sessionData.currentDay = data.currentDay;
        sessionData.isNight = data.isNight;
        sessionData.potionAwaitingDelivery = data.potionAwaitingDelivery;
        sessionData.savedNPCState = (NPCBrain.NPCState)data.savedNPCState;
        sessionData.hasNPCSpawnedToday = data.hasNPCSpawnedToday;
        sessionData.interactionCount = data.interactionCount;
        sessionData.plantDaysRemaining = new Dictionary<string, int>(data.plantDaysRemaining);

        if (sessionData.currentDay < sessionData.patientsPerDay.Count)
            sessionData.currentPatient = sessionData.patientsPerDay[sessionData.currentDay];

        // Tutorial Data
        if (data.isTutorialActive && tutorialData != null)
        {
            sessionData.tutorialData = tutorialData;
            tutorialData.tutorialStep = data.tutorialStep;
        }
        else
        {
            sessionData.tutorialData = null;
        }

        // Ineventory Data
        inventoryData.ClearData();
        foreach (KeyValuePair<string, int> pair in data.savedInv)
        {
            ItemDataSO itemEncontrado = allItemsDatabase.Find(i => i.name == pair.Key);
            
            if (itemEncontrado != null)
                inventoryData.savedInv.Add(itemEncontrado, pair.Value);
            else
                Debug.LogWarning($"Item {pair.Key} não encontrado no database!");
        }
        inventoryData.craftedPotions = new List<BrewedPotion>(data.craftedPotions);
        
        inventoryData.OnBeforeSerialize();
        sessionData.OnBeforeSerialize();

        Debug.Log($"Jogo carregado com sucesso de {SavePath}");
    }

    public bool HasSaveFile()
    {
        return File.Exists(SavePath);
    }
}