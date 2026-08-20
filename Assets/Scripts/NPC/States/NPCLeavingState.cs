using System.Collections;
using UnityEngine;

public sealed class NPCLeavingState : IState
{
    private readonly NPCBrain brain;
    private Coroutine leaveCoroutine;

    public NPCLeavingState(NPCBrain brain)
    {
        this.brain = brain;
    }

    public void Enter()
    {
        brain.sessionData.savedNPCState = NPCBrain.NPCStateEnum.LEAVING;

        brain.npcAnimator.SetHealed(brain.isHealed);

        if (brain.isHealed)
            brain.MovementScript.SetSpeed(brain.currentData.healedMovementSpeed);
        else
            brain.MovementScript.SetSpeed(brain.currentData.cursedMovementSpeed);

        leaveCoroutine = brain.StartCoroutine(LeaveSequence());
    }

    private IEnumerator LeaveSequence()
    {
        yield return new WaitForSeconds(1.5f);

        brain.npcAnimator.SetWalking(true, 1f);
        brain.MovementScript.MoveToDoor(() =>
        {
            brain.ChangeState(brain.HiddenState);
        });
    }

    public void Update() { }

    public void Exit()
    {
        if (leaveCoroutine != null) brain.StopCoroutine(leaveCoroutine);
    }
}