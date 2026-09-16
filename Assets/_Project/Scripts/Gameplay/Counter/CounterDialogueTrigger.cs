using JogoBruxinha.Core.SceneManagement;
using JogoBruxinha.Gameplay.GameFlow;
using JogoBruxinha.Gameplay.Tutorial;
using UnityEngine;

namespace JogoBruxinha.Gameplay.Counter
{
    public sealed class CounterDialogueTrigger : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private SessionDataSO _sessionData;

        [Header("UI References")]
        [SerializeField] private GameObject _counterCanvas;

        [Header("Managers")]
        [SerializeField] private PatientDeliveryManager _patientDelivery;
        [SerializeField] private TutorialManager _tutorialManager;

        private void Awake()
        {
            Debug.Assert(_sessionData != null, $"Missing _sessionData reference on {name}", this);
            Debug.Assert(_counterCanvas != null, $"Missing _counterCanvas reference on {name}", this);
            Debug.Assert(_patientDelivery != null, $"Missing _patientDelivery reference on {name}", this);
        }

        public void OpenDialogue()
        {
            if (FadeManager.Instance != null && FadeManager.Instance.IsFading) return;

            if (FadeManager.Instance == null)
            {
                ExecuteOpenDialogue();
                return;
            }

            FadeManager.Instance.StartFullFade(0f, 1f, ExecuteOpenDialogue);
        }

        private void ExecuteOpenDialogue()
        {
            if (_counterCanvas != null)
            {
                _counterCanvas.SetActive(true);
            }

            if (_sessionData.tutorialData != null)
            {
                if (_tutorialManager != null)
                {
                    Debug.Log("Balcão aberto no modo tutorial");
                    _tutorialManager.InteractWithTutorialCounter();
                }
                else
                {
                    Debug.LogWarning($"Tutorial ativo, mas _tutorialManager não referenciado em {name}!", this);
                }
                return;
            }

            PatientDataSO patient = _sessionData.currentPatient;
            if (patient != null)
            {
                Debug.Log($"Balcão aberto para atender paciente {patient.clientName}");
                HandlePatientDialogue(patient);
                return;
            }

            Debug.LogWarning($"Ninguém no balcão para conversar, mas a interação foi chamada em {name}!", this);
        }

        private void HandlePatientDialogue(PatientDataSO patient)
        {
            if (_sessionData.interactionCount == 0)
            {
                _patientDelivery.StartPatientEncounter(patient, patient.introDialogue);
            }
            else
            {
                int waitingIndex = _sessionData.interactionCount - 1;
                int maxIndex = patient.waitingDialogues.Count - 1;
                int safeIndex = Mathf.Min(waitingIndex, Mathf.Max(0, maxIndex));

                _patientDelivery.StartPatientEncounter(patient, patient.waitingDialogues[safeIndex].lines);
            }

            _sessionData.interactionCount++;
        }
    }
}