using UnityEngine;

public sealed class CompanionSleepingState : IState
{
    private readonly CompanionBrain brain;

    public CompanionSleepingState(CompanionBrain brain)
    {
        this.brain = brain;
    }

    public void Enter()
    {
        brain.MovementScript.StopFollowing();
    }

    public void Update()
    {
        Vector2 diff = (Vector2)brain.targetPlayer.position - (Vector2)brain.transform.position;
        float sqrDistance = diff.sqrMagnitude;

        float sqrDistanceToStopSleeping = brain.distanceToStopSleeping * brain.distanceToStopSleeping;
        if (sqrDistance > sqrDistanceToStopSleeping)
            brain.ChangeState(brain.IdleState);
    }

    public void Exit() {}

}
