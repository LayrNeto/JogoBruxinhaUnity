public sealed class NPCHiddenState : IState
{
    private readonly NPCBrain brain;

    public NPCHiddenState(NPCBrain brain)
    {
        this.brain = brain;
    }    

    public void Enter()
    {
        brain.SetVisibility(false);
        brain.npcAnimator.SetWalking(false, -1f);
        brain.npcAnimator.SetHealed(false);
        
        if (brain.counterInteractable) brain.counterInteractable.canInteract = false;
        brain.sessionData.savedNPCState = NPCBrain.NPCStateEnum.HIDDEN;
    }

    public void Update() { }
    public void Exit() { }
}