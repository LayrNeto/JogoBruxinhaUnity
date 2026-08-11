using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Header("Id")]
    public string spawnPointID;

    [Header("Dependencies")]
    public EntityTrackerSO tracker;
    public SessionDataSO sessionData;

    [Header("Player Settings")]
    public Vector2 dir;

    [Header("Companion Settings")]
    public Vector3 companionOffset = new Vector3(-1f, 0f, 0f); 
    public Vector2 companionDir;

    void Start()
    {
        if (FadeManager.Instance == null) 
        {
            Debug.LogWarning("ERROR: Missing FadeManager");
            return;
        }

        if (FadeManager.Instance.targetSpawnID == spawnPointID) 
        {
            if (spawnPointID == "SAVED_SPAWN")
            {
                SetupFromSaveData();
            }

            SpawnEntities();
        }
    }
    
    private void SetupFromSaveData()
    {
        transform.position = sessionData.playerPos;
        companionOffset = sessionData.companionPos - transform.position;
    }

    private void SpawnEntities()
    {
        if (tracker.player)
        {
            tracker.player.transform.position = transform.position;
            tracker.player.ChangeIdleDirection(dir);
        }
        
        if (tracker.companion != null)
        {
            tracker.companion.transform.position = transform.position + companionOffset;
            tracker.companion.ChangeIdleDirection(companionDir);
        }
    }

    private void OnDrawGizmos()
    {
        // Player 
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.3f);

        // Companion
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + companionOffset, 0.2f);
        
        // Line
        Gizmos.color = new Color(1, 1, 0, 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + companionOffset);
    }
}