using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))] 
public class Interactable : MonoBehaviour
{
    [Header("Interaction Settings")]
    public bool oneShot = false;
    public bool canInteract = true;
    
    [HideInInspector] 
    public bool isTimeAllowed = true;

    [Header("UI Key Position")]
    public Vector3 uiOffset = new Vector3(0, 1.5f, 0);

    [Header("Events")]
    public UnityEvent onInteract; 
    
    public UnityEvent onTargeted;
    public UnityEvent onUntargeted;

    public bool IsAvailable()
    {
        return canInteract && isTimeAllowed;
    }

    public void Interact()
    {
        if (!canInteract) return;

        onInteract?.Invoke();

        if (oneShot)
        {
            canInteract = false;
            
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
    }

    public void SetTargeted(bool targeted)
    {
        if (!canInteract) return;

        if (targeted)
            onTargeted?.Invoke();
        else
            onUntargeted?.Invoke();
    }

    public void SetInteractionState(bool interactionState)
    {
        canInteract = interactionState;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + uiOffset, 0.2f);
    }
}