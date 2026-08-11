using System.Collections.Generic;
using UnityEngine;

public class DayNightInteractablesManager : MonoBehaviour
{
    [Header("Dependencies")]
    public SessionDataSO sessionData;

    [Header("Interactables Configuration")]
    [Tooltip("Objetos que SÓ podem ser interagidos de DIA (ex: Balcão, Caldeirão)")]
    public List<Interactable> dayOnlyInteractables;
    
    [Tooltip("Objetos que SÓ podem ser interagidos de NOITE (ex: Cama)")]
    public List<Interactable> nightOnlyInteractables;


    private void Start()
    {
        ApplyTimeState();
    }

    public void ApplyTimeState()
    {
        if (sessionData == null) return;
        bool isNight = sessionData.isNight;

        foreach (Interactable interactable in dayOnlyInteractables)
        {
            if (interactable != null) interactable.isTimeAllowed = !isNight;
        }

        foreach (Interactable interactable in nightOnlyInteractables)
        {
            if (interactable != null) interactable.isTimeAllowed = isNight; 
        }
    }
}