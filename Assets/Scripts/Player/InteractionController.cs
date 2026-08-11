using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class InteractionController : MonoBehaviour
{
    [Header("UI Events")]
    public GameEventVector3 showUIEvent;
    public GameEvent hideUIEvent;

    private List<Interactable> interactablesInRange = new List<Interactable>();
    private Interactable currentTarget;

    void Start()
    {
        GameStateManager.Instance.inputControls.Player.Interact.performed += TryInteract;
    }

    void OnDestroy()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.inputControls.Player.Interact.performed -= TryInteract;
        }
    }

    void Update()
    {
        UpdateClosestTarget();
    }

    public void TryInteract(InputAction.CallbackContext context)
    {
        if (currentTarget != null && currentTarget.IsAvailable())
        {
            Interactable tempTarget = currentTarget;
            tempTarget.Interact();

            if (!tempTarget.IsAvailable())
            {
                if (currentTarget == tempTarget)
                {
                    ClearTarget();
                }
            }
        }
    }

    private void UpdateClosestTarget()
    {
        interactablesInRange.RemoveAll(item => item == null);

        if (interactablesInRange.Count == 0) { ClearTarget(); return; }

        Interactable closest = null;
        float minDistance = float.MaxValue;
        Vector2 playerPos = transform.position;

        foreach (Interactable item in interactablesInRange)
        {
            if (!item.IsAvailable()) continue;

            float dist = Vector2.Distance(playerPos, item.transform.position);

            if (dist < minDistance)
            {
                minDistance = dist;
                closest = item;
            }
        }

        if (closest != currentTarget)
        {
            if (closest != null) SetNewTarget(closest);
            else ClearTarget();
        }
    }

    private void SetNewTarget(Interactable target)
    {
        if (currentTarget)
        {
            currentTarget.SetTargeted(false);
        }

        currentTarget = target;

        if (currentTarget)
        {
            currentTarget.SetTargeted(true);
            
            Vector3 uiPos = currentTarget.transform.position + currentTarget.uiOffset;
            showUIEvent?.Raise(uiPos);
        }
        else
        {
            hideUIEvent?.Raise();
        }
    }

    public void ClearTarget()
    {
        if (currentTarget != null)
        {
            currentTarget.SetTargeted(false);
            currentTarget = null;
            hideUIEvent?.Raise();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Interactable interactable = collision.GetComponent<Interactable>();
        if (interactable != null && !interactablesInRange.Contains(interactable))
        {
            interactablesInRange.Add(interactable);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Interactable interactable = collision.GetComponent<Interactable>();
        if (interactable != null)
        {
            interactablesInRange.Remove(interactable);
            if (interactable == currentTarget) ClearTarget();
        }
    }
}