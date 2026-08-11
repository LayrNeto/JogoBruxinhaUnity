using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HouseManager : MonoBehaviour
{
    [Header("Dependencies")]
    public NPCBrain npcBrain; 
    public TriggerArea npcTriggerArea;
    public TriggerArea doorbellArea;
    
    [Header("Daily Data")]
    public SessionDataSO sessionData;

    private void Start()
    {
        if (sessionData && sessionData.hasNPCSpawnedToday)
        {
            if (npcTriggerArea) npcTriggerArea.gameObject.SetActive(false);
            if (doorbellArea) doorbellArea.gameObject.SetActive(false);
        }

        if (npcBrain && sessionData)
        {
            npcBrain.SetupNPC(sessionData.currentPatient);
            Debug.Log("NPC configurado e aguardando o trigger do jogador!");
        }
    }

    public void SleepAndAdvance()
    {
        Debug.Log("Dormindo... Iniciando fade out.");
        string currentScene = SceneManager.GetActiveScene().name;
        
        FadeManager.Instance.StartTransition(currentScene, "BedSpawn", 1.5f, 1.5f, () => 
        {
            sessionData.AdvanceDay();
            Debug.Log($"Acordando no dia {sessionData.currentDay}");
        });
    }
}