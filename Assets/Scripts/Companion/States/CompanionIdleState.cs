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
    Vector2 diff = (Vector2)brain.targetPlayer.position - (Vector2)brain.transform.position;
    float sqrDistance = diff.sqrMagnitude;

    float sqrDistanceToFollow = brain.distanceToStartFollowing * brain.distanceToStartFollowing;
    if (sqrDistance > sqrDistanceToFollow)
    {
        brain.ChangeState(brain.FollowingState);
        return;
    }

    idleTimer += Time.deltaTime;

    float sqrDistanceToSleep = brain.distanceToStopSleeping * brain.distanceToStopSleeping;
    if (idleTimer >= brain.timeToFallAsleep && sqrDistance <= sqrDistanceToSleep)
    {
        brain.ChangeState(brain.SleepingState);
    }
}

    public void Exit() {}

}
