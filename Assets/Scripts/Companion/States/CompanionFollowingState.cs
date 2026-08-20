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
        float distance = Vector2.Distance(brain.transform.position, brain.targetPlayer.transform.position);

        if (distance <= brain.distanceToStop)
            brain.ChangeState(brain.IdleState);
    }

    public void Exit()
    {
        brain.MovementScript.StopFollowing();
    }

}
