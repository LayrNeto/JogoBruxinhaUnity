public sealed class NPCEnteringState : IState
{
    private readonly NPCBrain brain;

    public NPCEnteringState(NPCBrain brain)
    {
        this.brain = brain;
    }

    public void Enter()
    {
        brain.SetVisibility(true);
        brain.sessionData.hasNPCSpawnedToday = true;
        brain.sessionData.savedNPCState = NPCBrain.NPCStateEnum.ENTERING;

        brain.npcAnimator.SetWalking(true, -1f);
        brain.MovementScript.MoveToCounter(() =>
        {
            brain.ChangeState(brain.WaitingState);
        });
    }

    public void Update() { }
    
    public void Exit()
    {
        brain.npcAnimator.SetWalking(false, -1f);
    }
}