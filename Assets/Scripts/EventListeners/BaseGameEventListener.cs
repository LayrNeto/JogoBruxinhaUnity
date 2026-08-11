using UnityEngine;
using UnityEngine.Events;

public abstract class BaseGameEventListener<T> : MonoBehaviour
{
    public BaseGameEvent<T> gameEvent;
    public UnityEvent<T> response;

    private void OnEnable() 
    { 
        if (gameEvent != null) gameEvent.listeners += EventResponse; 
    }

    private void OnDisable() 
    { 
        if (gameEvent != null) gameEvent.listeners -= EventResponse; 
    }
    
    private void EventResponse(T item) 
    { 
        response?.Invoke(item); 
    }
}