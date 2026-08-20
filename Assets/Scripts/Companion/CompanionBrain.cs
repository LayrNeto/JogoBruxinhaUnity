using System;
using UnityEngine;

[RequireComponent(typeof(Interactable))]
[RequireComponent(typeof(CompanionMovement))]
public class CompanionBrain : MonoBehaviour
{
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

    public IState IdleState { get; private set; }
    public IState FollowingState { get; private set; }
    public IState SleepingState { get; private set; }
    public IState CurrentState { get; private set; }
    
    public CompanionMovement MovementScript { get; private set; }
    private Interactable interactable;
    private bool isMovementBlocked;

    private void Awake()
    {
        MovementScript = GetComponent<CompanionMovement>();
        interactable = GetComponent<Interactable>();

        IdleState = new CompanionIdleState(this);
        FollowingState = new CompanionFollowingState(this);
        SleepingState = new CompanionSleepingState(this);
    }

    private void OnEnable()
    {
        entityTracker.companion = this; 
    }

    private void OnDisable()
    {
        if (entityTracker.companion == this) 
            entityTracker.companion = null;
    }

    void Start()
    {
        if (sessionData && sessionData.isNight)
        {
            GoSleepForTheNight();
            return;
        }

        ChangeState(IdleState);
    }

    void Update()
    {
        if (isMovementBlocked || targetPlayer == null) return;

        CurrentState?.Update();
    }

    public void ChangeState(IState newState)
    {
        if (newState == null || CurrentState == newState) return;

        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
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
        CurrentState = SleepingState;
        
        if (MovementScript != null) MovementScript.DisableMovement();
        if (childAnimator != null) childAnimator.SleepForTheNight();

        transform.position = bedTranform.position;
        interactable.canInteract = false;

        enabled = false;
    }
}