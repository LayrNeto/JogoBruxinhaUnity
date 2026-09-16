using System.Collections.Generic;
using JogoBruxinha.Gameplay.GameFlow;
using UnityEngine;

namespace JogoBruxinha.Gameplay.Environment
{
    public sealed class DayNightInteractablesManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private SessionDataSO _sessionData;

        [Header("Interactables Configuration")]
        [Tooltip("Objetos que SÓ podem ser interagidos de DIA (ex: Balcão, Caldeirão)")]
        [SerializeField] private List<Interactable> _dayOnlyInteractables = new List<Interactable>();

        [Tooltip("Objetos que SÓ podem ser interagidos de NOITE (ex: Cama)")]
        [SerializeField] private List<Interactable> _nightOnlyInteractables = new List<Interactable>();

        private void Awake()
        {
            Debug.Assert(_sessionData != null, $"Missing _sessionData reference on {name}", this);
        }

        private void Start()
        {
            ApplyTimeState();
        }

        public void ApplyTimeState()
        {
            if (_sessionData == null) return;
            bool isNight = _sessionData.isNight;

            if (_dayOnlyInteractables != null)
            {
                foreach (Interactable interactable in _dayOnlyInteractables)
                {
                    if (interactable != null)
                    {
                        interactable.isTimeAllowed = !isNight;
                    }
                }
            }

            if (_nightOnlyInteractables != null)
            {
                foreach (Interactable interactable in _nightOnlyInteractables)
                {
                    if (interactable != null)
                    {
                        interactable.isTimeAllowed = isNight;
                    }
                }
            }
        }
    }
}