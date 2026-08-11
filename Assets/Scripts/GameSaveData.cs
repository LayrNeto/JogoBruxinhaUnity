using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct SaveVector3
{
    public float x, y, z;

    public SaveVector3(Vector3 unityVector)
    {
        x = unityVector.x;
        y = unityVector.y;
        z = unityVector.z;
    }

    public Vector3 ToUnityVector()
    {
        return new Vector3(x, y, z);
    }
}

public class GameSaveData
{
    // Tutorial Data
    public bool isTutorialActive;
    public int tutorialStep;

    // Sesion Data
    public string currentScene;
    public SaveVector3 playerPos;
    public SaveVector3 companionPos;
    public bool isNight;
    public int currentDay; 
    public bool potionAwaitingDelivery;
    public int savedNPCState;
    public bool hasNPCSpawnedToday;
    public int interactionCount;
    public Dictionary<string, int> plantDaysRemaining = new Dictionary<string, int>();

    // Inventory Data
    public Dictionary<string, int> savedInv = new Dictionary<string, int>();
    public List<BrewedPotion> craftedPotions = new List<BrewedPotion>();
}