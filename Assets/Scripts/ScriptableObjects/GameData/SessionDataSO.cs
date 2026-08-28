using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSessionData", menuName = "Scriptable Objects/Game Data/Session Data")]
public class SessionDataSO : ScriptableObject, ISerializationCallbackReceiver
{
    [Header("Modules")]
    [Tooltip("Se for nulo, o jogo roda normalmente. Se for atibuido, roda o tutorial.")]
    public TutorialDataSO tutorialData;

    [Header("Time & Progression")]
    public bool isNight = false;
    public int currentDay = 0; 
    public List<PatientDataSO> patientsPerDay; 

    [Header("Location Data")]
    public string currentScene;
    public Vector3 playerPos;
    public Vector3 companionPos;
    
    [Header("Potion Prepared")]
    public bool potionAwaitingDelivery = false;

    [Header("NPC Daily State")]
    public PatientDataSO currentPatient;
    public NPCBrain.NPCStateEnum savedNPCState = NPCBrain.NPCStateEnum.HIDDEN;
    public bool hasNPCSpawnedToday = false;
    public int interactionCount = 0;

    [Header("Greenhouse Data")]
    public Dictionary<string, int> plantDaysRemaining = new Dictionary<string, int>();

    [Header("Greenhouse [Inspector View]")]
    public List<string> plantKeys = new List<string>();
    public List<int> plantValues = new List<int>();

    [ContextMenu("Reset Session Data")]
    public void ResetSession()
    {
        currentDay = 0;

        currentScene = string.Empty;

        playerPos = Vector3.zero;
        companionPos = Vector3.zero;

        savedNPCState = NPCBrain.NPCStateEnum.HIDDEN;
        potionAwaitingDelivery = false;
        hasNPCSpawnedToday = false;
        isNight = false;
        interactionCount = 0;

        plantDaysRemaining.Clear();
        plantKeys.Clear();
        plantValues.Clear();

        tutorialData = null;

        if (patientsPerDay != null && patientsPerDay.Count > 0)
        {
            currentPatient = patientsPerDay[0];
        }
        else
        {
            Debug.LogWarning("A lista de pacientes está vazia no SessionDataSO!");
        }
    }

    public void AdvanceDay()
    {
        currentDay++;
        
        savedNPCState = NPCBrain.NPCStateEnum.HIDDEN;
        potionAwaitingDelivery = false;
        hasNPCSpawnedToday = false;
        isNight = false;
        interactionCount = 0;

        List<string> keys = new List<string>(plantDaysRemaining.Keys);
        foreach (string key in keys)
        {
            if (plantDaysRemaining[key] > 0)
            {
                plantDaysRemaining[key]--;
            }
        }

        if (currentDay < patientsPerDay.Count)
        {
            currentPatient = patientsPerDay[currentDay];
        }
        else
        {
            Debug.Log("Acabaram os dias planejados! Fim de jogo");
        }

        Debug.Log("Avançando um dia");
    }

    public void OnBeforeSerialize()
    {
        plantKeys.Clear();
        plantValues.Clear();

        foreach (KeyValuePair<string, int> pair in plantDaysRemaining)
        {
            plantKeys.Add(pair.Key);
            plantValues.Add(pair.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        plantDaysRemaining.Clear();

        if (plantKeys.Count != plantValues.Count) return;

        for (int i = 0; i < plantKeys.Count; i++)
        {
            if (!plantDaysRemaining.ContainsKey(plantKeys[i]))
            {
                plantDaysRemaining.Add(plantKeys[i], plantValues[i]);
            }
        }
    }

    public void SetPlantGrowth(string instanceID, int days)
    {
        if (plantDaysRemaining.ContainsKey(instanceID))
        {
            plantDaysRemaining[instanceID] = days;
        }
        else
        {
            plantDaysRemaining.Add(instanceID, days);
        }
    }
}