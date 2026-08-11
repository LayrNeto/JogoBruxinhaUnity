using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CounterDialogueTrigger : MonoBehaviour
{
    [Header("Dependencies")]
    public SessionDataSO sessionData;

    [Header("UI References")]
    public GameObject counterCanvas;
    
    [Header("Managers")]
    public PatientDeliveryManager patientDelivery;
    public TutorialManager tutorialManager; 

    public void OpenDialogue()
    {
        FadeManager.Instance.StartFullFade(0f, 1f, () => 
        {
            if (counterCanvas) counterCanvas.SetActive(true);

            if (sessionData.tutorialData)
            {
                if (tutorialManager)
                {
                    Debug.Log("Balcão aberto no modo tutorial");
                    tutorialManager.InteractWithTutorialCounter();

                }
                else
                {
                    Debug.LogWarning("Tutorial ativo, mas TutorialManager não referenciado no Balcão!");
                }
                return;
            }

            PatientDataSO patient = sessionData.currentPatient;
            if (patient != null)
            {
                Debug.Log($"Balcão aberto para atender paciente {patient.clientName}");
                HandlePatientDialogue(patient);
                return;
            }

            Debug.LogWarning("Ninguém no balcão para conversar, mas a interação foi chamada!");
        });
    }

    private void HandlePatientDialogue(PatientDataSO patient)
    {
        if (sessionData.interactionCount == 0)
        {
            patientDelivery.StartPatientEncounter(patient, patient.introDialogue);
        }
        else
        {
            int waitingIndex = sessionData.interactionCount - 1;
            int maxIndex = patient.waitingDialogues.Count - 1;
            int safeIndex = Mathf.Min(waitingIndex, maxIndex);

            patientDelivery.StartPatientEncounter(patient, patient.waitingDialogues[safeIndex].lines);
        }

        sessionData.interactionCount++;
    }
}