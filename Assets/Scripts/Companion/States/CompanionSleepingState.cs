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
        float distance = Vector2.Distance(brain.transform.position, brain.targetPlayer.transform.position);

        if (distance > brain.distanceToStopSleeping)
            brain.ChangeState(brain.IdleState);
    }

    public void Exit() {}

}
