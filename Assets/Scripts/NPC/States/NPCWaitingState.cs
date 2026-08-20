public sealed class NPCWaitingState : IState
{
    private readonly NPCBrain brain;

    public NPCWaitingState(NPCBrain brain)
    {
        this.brain = brain;
    }

    public void Enter()
    {
        brain.SetVisibility(true);
        brain.sessionData.savedNPCState = NPCBrain.NPCStateEnum.WAITING;
        brain.npcAnimator.SetWalking(false, -1f);
        brain.npcAnimator.SetHealed(brain.isHealed);

        if (brain.counterInteractable) brain.counterInteractable.canInteract = true;
    }

    public void Update() { }
    public void Exit()
    {
        if (brain.counterInteractable) brain.counterInteractable.canInteract = false;
    }
}