using UnityEngine;

public class CompanionBrain : MonoBehaviour
{
    public enum CompanionState { Idle, Following, Sleeping }
    public CompanionState currentState = CompanionState.Idle;
    

    [Header("References")]
    public EntityTrackerSO entityTracker;
    public SessionDataSO sessionData;
    public Transform targetPlayer;
    public CompanionAnimator childAnimator;

    [Header("Distance Rules")]
    public float distanceToStartFollowing = 4f; 
    public float distanceToStop = 1.5f;    

    [Header("Sleep Rules")]
    public float timeToFallAsleep = 5f;    
    public float distanceToStopSleeping = 2f;      

    [Header("Target Tranforms")]
    public Transform bedTranform;
    
    private CompanionMovement movementScript;
    private float idleTimer = 0f;             
    private bool isMovementBlocked;
    private Interactable interactable;

    private void OnEnable()
    {
        entityTracker.companion = this; 
    }

    private void OnDisable()
    {
        if (entityTracker.companion == this) entityTracker.companion = null;
    }

    void Start()
    {
        movementScript = GetComponent<CompanionMovement>();
        interactable = GetComponent<Interactable>();

        if (sessionData && sessionData.isNight) GoSleepForTheNight();
    }

    void Update()
    {
        if (isMovementBlocked || targetPlayer == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, targetPlayer.position);

        switch (currentState)
        {
            case CompanionState.Idle:
                StateIdle(distanceToPlayer);
                break;

            case CompanionState.Following:
                StateFollowing(distanceToPlayer);
                break;

            case CompanionState.Sleeping:
                StateSleeping(distanceToPlayer);
                break;
        }
    }

    private void StateIdle(float distance)
    {
        movementScript.StopFollowing();

        if (distance > distanceToStartFollowing)
        {
            currentState = CompanionState.Following;
            idleTimer = 0f;
        }
        else
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= timeToFallAsleep && distance <= distanceToStopSleeping)
            {
                currentState = CompanionState.Sleeping;
            }
        }
    }

    private void StateFollowing(float distance)
    {
        movementScript.StartFollowing();

        if (distance <= distanceToStop)
        {
            currentState = CompanionState.Idle;
            idleTimer = 0f;
        }
    }

    private void StateSleeping(float distance)
    {
        movementScript.StopFollowing();

        if (distance > distanceToStopSleeping)
        {
            currentState = CompanionState.Idle;
            idleTimer = 0f;
        }
    }

    public void ChangeIdleDirection(Vector2 newDir)
    {
        if (childAnimator)
        {
            childAnimator.ForceDirection(newDir.x);
        }
        else
        {
            Debug.LogWarning("CompanionAnimator não referenciado no CompanionBrain!");
        }
    }

    public void GoSleepForTheNight()
    {
        currentState = CompanionState.Sleeping;
        
        if (movementScript != null) movementScript.DisableMovement();
        if (childAnimator != null) childAnimator.SleepForTheNight();

        transform.position = bedTranform.position;
        interactable.canInteract = false;

        enabled = false;
    }
}