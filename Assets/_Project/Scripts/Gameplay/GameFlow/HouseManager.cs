using JogoBruxinha.Core.Audio;
using JogoBruxinha.Core.SceneManagement;
using JogoBruxinha.Gameplay.Characters.NPC;
using JogoBruxinha.Gameplay.Environment;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JogoBruxinha.Gameplay.GameFlow
{
    public sealed class HouseManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private NPCBrain _npcBrain;
        [SerializeField] private TriggerArea _npcTriggerArea;
        [SerializeField] private TriggerArea _doorbellArea;

        [Header("Daily Data")]
        [SerializeField] private SessionDataSO _sessionData;

        private void Awake()
        {
            Debug.Assert(_npcBrain != null, $"Missing _npcBrain reference on {name}", this);
            Debug.Assert(_npcTriggerArea != null, $"Missing _npcTriggerArea reference on {name}", this);
            Debug.Assert(_doorbellArea != null, $"Missing _doorbellArea reference on {name}", this);
            Debug.Assert(_sessionData != null, $"Missing _sessionData reference on {name}", this);
        }

        private void Start()
        {
            if (_sessionData != null && _sessionData.hasNPCSpawnedToday)
            {
                if (_npcTriggerArea != null) _npcTriggerArea.gameObject.SetActive(false);
                if (_doorbellArea != null) _doorbellArea.gameObject.SetActive(false);
            }

            if (_npcBrain != null && _sessionData != null)
            {
                _npcBrain.SetupNPC(_sessionData.currentPatient);
                Debug.Log("NPC configurado e aguardando o trigger do jogador!");
            }
        }

        public void SleepAndAdvance()
        {
            Debug.Log("Dormindo... Iniciando fade out.");
            string currentScene = SceneManager.GetActiveScene().name;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopMusic(1f);
            }

            if (FadeManager.Instance != null)
            {
                FadeManager.Instance.StartTransition(currentScene, "BedSpawn", 1.5f, 1.5f, () =>
                {
                    if (_sessionData != null)
                    {
                        _sessionData.AdvanceDay();
                        Debug.Log($"Acordando no dia {_sessionData.currentDay}");
                    }
                });
            }
            else if (_sessionData != null)
            {
                _sessionData.AdvanceDay();
            }
        }
    }
}