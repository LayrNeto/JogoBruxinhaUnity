using UnityEngine;
using UnityEngine.Events;

public abstract class BaseGameEvent<T> : ScriptableObject, ISerializationCallbackReceiver
{
    public UnityAction<T> listeners;

    public void Raise(T item)
    {
        listeners?.Invoke(item);
    }

    private void OnEnable()
    {
        listeners = null;
    }

    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize()
    {
        listeners = null;
    }
}