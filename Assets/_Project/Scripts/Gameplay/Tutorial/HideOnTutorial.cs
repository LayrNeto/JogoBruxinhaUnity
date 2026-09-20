using JogoBruxinha.Gameplay.GameFlow;
using UnityEngine;

public sealed class HideOnTutorial : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private SessionDataSO _sessionData;

    void Awake()
    {
        Debug.Assert(_sessionData != null, $"Missing _sessionData reference on {name}", this);    
    }

    void OnEnable()
    {
        if (_sessionData.tutorialData != null)
            gameObject.SetActive(false);
    }
}
