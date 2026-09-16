using JogoBruxinha.Core.StateMachines;

namespace JogoBruxinha.Gameplay.Characters.NPC
{
    public sealed class NPCWaitingState : IState
    {
        private readonly NPCBrain _brain;

        public NPCWaitingState(NPCBrain brain)
        {
            _brain = brain;
        }

        public void Enter()
        {
            _brain.SetVisibility(true);

            if (_brain.SessionData != null)
            {
                _brain.SessionData.savedNPCState = NPCBrain.NPCStateEnum.WAITING;
            }

            if (_brain.NpcAnimator != null)
            {
                _brain.NpcAnimator.SetWalking(false, -1f);
                _brain.NpcAnimator.SetHealed(_brain.IsHealed);
            }

            if (_brain.CounterInteractable != null)
            {
                _brain.CounterInteractable.canInteract = true;
            }
        }

        public void Update() { }

        public void Exit()
        {
            if (_brain.CounterInteractable != null)
            {
                _brain.CounterInteractable.canInteract = false;
            }
        }
    }
}