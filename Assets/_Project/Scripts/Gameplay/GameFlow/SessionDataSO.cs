using System.Collections.Generic;
using JogoBruxinha.Gameplay.Characters.NPC;
using JogoBruxinha.Gameplay.Counter;
using JogoBruxinha.Gameplay.Tutorial;
using UnityEngine;

namespace JogoBruxinha.Gameplay.GameFlow
{
    [CreateAssetMenu(fileName = "NewSessionData", menuName = "Scriptable Objects/Game Data/Session Data")]
    public sealed class SessionDataSO : ScriptableObject, ISerializationCallbackReceiver
    {
        [Header("Modules")]
        [Tooltip("Se for nulo, o jogo roda normalmente. Se for atribuído, roda o tutorial.")]
        public TutorialDataSO tutorialData;

        [Header("Time & Progression")]
        public bool isNight = false;
        public int currentDay = 0;
        public List<PatientDataSO> patientsPerDay = new List<PatientDataSO>();

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
        [SerializeField] private List<string> _plantKeys = new List<string>();
        [SerializeField] private List<int> _plantValues = new List<int>();

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
            _plantKeys.Clear();
            _plantValues.Clear();

            tutorialData = null;

            if (patientsPerDay != null && patientsPerDay.Count > 0)
            {
                currentPatient = patientsPerDay[0];
            }
            else
            {
                Debug.LogWarning("A lista de pacientes está vazia no SessionDataSO!", this);
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

            if (patientsPerDay != null && currentDay < patientsPerDay.Count)
            {
                currentPatient = patientsPerDay[currentDay];
            }
            else
            {
                Debug.Log("Acabaram os dias planejados! Fim de jogo.");
            }

            Debug.Log("Avançando um dia");
        }

        public void OnBeforeSerialize()
        {
            _plantKeys.Clear();
            _plantValues.Clear();

            foreach (KeyValuePair<string, int> pair in plantDaysRemaining)
            {
                _plantKeys.Add(pair.Key);
                _plantValues.Add(pair.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            plantDaysRemaining.Clear();

            if (_plantKeys.Count != _plantValues.Count) return;

            for (int i = 0; i < _plantKeys.Count; i++)
            {
                if (!plantDaysRemaining.ContainsKey(_plantKeys[i]))
                {
                    plantDaysRemaining.Add(_plantKeys[i], _plantValues[i]);
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
}