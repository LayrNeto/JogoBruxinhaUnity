using UnityEngine;
using UnityEngine.Events;

public abstract class BaseGameEvent<T> : ScriptableObject
{
    public UnityAction<T> listeners;
    
    public void Raise(T item) 
    { 
        listeners?.Invoke(item); 
    }
}