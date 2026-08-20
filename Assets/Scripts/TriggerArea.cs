using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class TriggerArea : MonoBehaviour
{
    [Header("Trigger Settings")]
    public string targetTag = "Player";
    public bool oneShot = true;
    private bool hasTriggered = false;

    [Header("Audio")]
    public SoundDataSO triggerAudio;

    [Header("Events")]
    public UnityEvent onTriggerEnterEvent;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered && oneShot) return;

        if (other.CompareTag(targetTag))
        {
            if (triggerAudio) AudioManager.Instance.PlaySFX(triggerAudio);
            
            onTriggerEnterEvent?.Invoke();

            if (oneShot)
            {
                hasTriggered = true;
                
                Collider2D col = GetComponent<Collider2D>();
                if (col != null) col.enabled = false;
            }
        }
    }
}