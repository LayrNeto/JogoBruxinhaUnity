using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/Game Event")]
public class GameEvent : ScriptableObject
{
    public UnityAction listeners; 

    public void Raise()
    {
        listeners?.Invoke();
    }
}