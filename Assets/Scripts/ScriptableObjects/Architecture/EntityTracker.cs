using UnityEngine;

[CreateAssetMenu(fileName = "EntityTracker", menuName = "Scriptable Objects/Architecture/Entity Tracker")]
public class EntityTrackerSO : ScriptableObject
{
    [Header("Runtime References [READ ONLY]")]
    public PlayerController player;
    public CompanionBrain companion;

    private void OnDisable()
    {
        player = null;
        companion = null;
    }
}