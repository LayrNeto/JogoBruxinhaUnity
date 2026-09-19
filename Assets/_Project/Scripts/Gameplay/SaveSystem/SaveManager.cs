using System.Collections.Generic;
using System.IO;
using JogoBruxinha.Gameplay.Characters.NPC;
using JogoBruxinha.Gameplay.GameFlow;
using JogoBruxinha.Gameplay.Inventory;
using JogoBruxinha.Gameplay.Tutorial;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JogoBruxinha.Gameplay.SaveSystem
{
    public sealed class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        [Header("Scriptable Objects")]
        [SerializeField] private SessionDataSO _sessionData;
        [SerializeField] private TutorialDataSO _tutorialData;
        [SerializeField] private InventoryDataSO _inventoryData;

        [Header("Dependencies")]
        [SerializeField] private EntityTrackerSO _tracker;

        [Header("All Items Database")]
        [SerializeField] private List<ItemDataSO> _allItemsDatabase = new List<ItemDataSO>();

        private string SavePath => Path.Combine(Application.persistentDataPath, "slot_01.json");

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            Debug.Assert(_sessionData != null, $"Missing _sessionData reference on {name}", this);
            Debug.Assert(_inventoryData != null, $"Missing _inventoryData reference on {name}", this);
            Debug.Assert(_tracker != null, $"Missing _tracker reference on {name}", this);
            Debug.Assert(_allItemsDatabase != null && _allItemsDatabase.Count > 0, $"Missing or empty _allItemsDatabase on {name}", this);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void SaveGame()
        {
            if (_sessionData == null || _inventoryData == null || _tracker == null)
            {
                Debug.LogError($"Não foi possível salvar o jogo: dependências ausentes em {name}!", this);
                return;
            }

            _sessionData.currentScene = SceneManager.GetActiveScene().name;

            if (_tracker.Player != null)
            {
                _sessionData.playerPos = _tracker.Player.transform.position;
            }

            if (_tracker.Companion != null)
            {
                _sessionData.companionPos = _tracker.Companion.transform.position;
            }

            GameSaveData data = new GameSaveData
            {
                // Session Data
                currentScene = _sessionData.currentScene,
                playerPos = new SaveVector3(_sessionData.playerPos),
                companionPos = new SaveVector3(_sessionData.companionPos),
                currentDay = _sessionData.currentDay,
                vitalEnergy = _sessionData.VitalEnergy,
                isNight = _sessionData.isNight,
                potionAwaitingDelivery = _sessionData.potionAwaitingDelivery,
                savedNPCState = (int)_sessionData.savedNPCState,
                hasNPCSpawnedToday = _sessionData.hasNPCSpawnedToday,
                interactionCount = _sessionData.interactionCount,
                plantDaysRemaining = new Dictionary<string, int>(_sessionData.plantDaysRemaining),

                // Tutorial Data
                isTutorialActive = _sessionData.tutorialData != null
            };

            if (data.isTutorialActive && _tutorialData != null)
            {
                data.tutorialStep = _tutorialData.tutorialStep;
            }

            // Inventory Data
            foreach (KeyValuePair<ItemDataSO, int> pair in _inventoryData.savedInv)
            {
                if (pair.Key != null)
                {
                    data.savedInv[pair.Key.name] = pair.Value;
                }
            }

            data.craftedPotions = new List<BrewedPotion>(_inventoryData.craftedPotions);

            string directoryPath = Path.GetDirectoryName(SavePath);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(SavePath, json);

            Debug.Log($"Jogo salvo com sucesso em: {SavePath}");
        }

        public void LoadGame()
        {
            if (!File.Exists(SavePath))
            {
                Debug.LogWarning($"Arquivo de save não encontrado em {SavePath}!");
                return;
            }

            string json = File.ReadAllText(SavePath);
            GameSaveData data = JsonConvert.DeserializeObject<GameSaveData>(json);

            if (data == null)
            {
                Debug.LogError($"Falha ao ler os dados do save em {SavePath}!");
                return;
            }

            // Session Data
            _sessionData.currentScene = data.currentScene;
            _sessionData.playerPos = data.playerPos.ToUnityVector();
            _sessionData.companionPos = data.companionPos.ToUnityVector();
            _sessionData.currentDay = data.currentDay;
            _sessionData.RestoreVitalEnergy(data.vitalEnergy);
            _sessionData.isNight = data.isNight;
            _sessionData.potionAwaitingDelivery = data.potionAwaitingDelivery;
            _sessionData.savedNPCState = (NPCBrain.NPCStateEnum)data.savedNPCState;
            _sessionData.hasNPCSpawnedToday = data.hasNPCSpawnedToday;
            _sessionData.interactionCount = data.interactionCount;
            _sessionData.plantDaysRemaining = data.plantDaysRemaining != null 
                ? new Dictionary<string, int>(data.plantDaysRemaining) 
                : new Dictionary<string, int>();

            if (_sessionData.patientsPerDay != null && _sessionData.currentDay < _sessionData.patientsPerDay.Count)
            {
                _sessionData.currentPatient = _sessionData.patientsPerDay[_sessionData.currentDay];
            }

            // Tutorial Data
            if (data.isTutorialActive && _tutorialData != null)
            {
                _sessionData.tutorialData = _tutorialData;
                _tutorialData.tutorialStep = data.tutorialStep;
            }
            else
            {
                _sessionData.tutorialData = null;
            }

            // Inventory Data
            _inventoryData.ClearData();

            if (data.savedInv != null)
            {
                foreach (KeyValuePair<string, int> pair in data.savedInv)
                {
                    ItemDataSO itemEncontrado = _allItemsDatabase.Find(i => i != null && i.name == pair.Key);

                    if (itemEncontrado != null)
                    {
                        _inventoryData.savedInv[itemEncontrado] = pair.Value;
                    }
                    else
                    {
                        Debug.LogWarning($"Item '{pair.Key}' não encontrado no database ao carregar save!", this);
                    }
                }
            }

            if (data.craftedPotions != null)
            {
                _inventoryData.craftedPotions = new List<BrewedPotion>(data.craftedPotions);
            }

            _inventoryData.OnBeforeSerialize();
            _sessionData.OnBeforeSerialize();

            Debug.Log($"Jogo carregado com sucesso de {SavePath}");
        }

        public bool HasSaveFile()
        {
            return File.Exists(SavePath);
        }
    }
}
