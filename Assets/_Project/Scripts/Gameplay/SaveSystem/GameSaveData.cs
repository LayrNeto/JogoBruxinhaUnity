using System;
using System.Collections.Generic;
using JogoBruxinha.Gameplay.Inventory;
using UnityEngine;

namespace JogoBruxinha.Gameplay.SaveSystem
{
    [Serializable]
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

    [Serializable]
    public sealed class GameSaveData
    {
        // Tutorial Data
        public bool isTutorialActive;
        public int tutorialStep;

        // Session Data
        public string currentScene;
        public SaveVector3 playerPos;
        public SaveVector3 companionPos;
        public bool isNight;
        public int currentDay;
        // Field initializer keeps saves from before the energy system compatible.
        public int vitalEnergy = 100;
        public bool potionAwaitingDelivery;
        public int savedNPCState;
        public bool hasNPCSpawnedToday;
        public int interactionCount;
        public Dictionary<string, int> plantDaysRemaining = new Dictionary<string, int>();

        // Inventory Data
        public Dictionary<string, int> savedInv = new Dictionary<string, int>();
        public List<BrewedPotion> craftedPotions = new List<BrewedPotion>();
    }
}
