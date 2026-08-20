using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/Game Event")]
public class GameEvent : ScriptableObject, ISerializationCallbackReceiver
{
    public UnityAction listeners;

    public void Raise()
    {
        listeners?.Invoke();
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