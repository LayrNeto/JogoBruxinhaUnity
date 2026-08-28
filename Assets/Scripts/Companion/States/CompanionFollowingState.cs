using UnityEngine;

public sealed class CompanionFollowingState : IState
{
    private readonly CompanionBrain brain;

    public CompanionFollowingState(CompanionBrain brain)
    {
        this.brain = brain;
    }

    public void Enter()
    {
        brain.MovementScript.StartFollowing();
    }

    public void Update()
    {
        Vector2 diff = (Vector2)brain.targetPlayer.position - (Vector2)brain.transform.position;
        float sqrDistance = diff.sqrMagnitude;

        float sqrDistanceToStop = brain.distanceToStop * brain.distanceToStop;
        if (sqrDistance <= sqrDistanceToStop)
            brain.ChangeState(brain.IdleState);
    }

    public void Exit()
    {
        brain.MovementScript.StopFollowing();
    }

}
