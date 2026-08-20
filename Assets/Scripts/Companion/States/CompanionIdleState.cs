using UnityEngine;

public sealed class CompanionIdleState : IState
{
    private readonly CompanionBrain brain;
    private float idleTimer;

    public CompanionIdleState(CompanionBrain brain)
    {
        this.brain = brain;
    }

    public void Enter()
    {
        idleTimer = 0f;
        brain.MovementScript.StopFollowing();
    }

    public void Update()
    {
        float distance = Vector2.Distance(brain.transform.position, brain.targetPlayer.transform.position);

        if (distance > brain.distanceToStartFollowing)
        {
            brain.ChangeState(brain.FollowingState);
            return;
        }

        idleTimer += Time.deltaTime;
        
        if (idleTimer >= brain.timeToFallAsleep && distance <= brain.distanceToStopSleeping)
            brain.ChangeState(brain.SleepingState);
    }

    public void Exit() {}

}
