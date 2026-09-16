using JogoBruxinha.Core.StateMachines;

namespace JogoBruxinha.Gameplay.Characters.NPC
{
    public sealed class NPCHiddenState : IState
    {
        private readonly NPCBrain _brain;

        public NPCHiddenState(NPCBrain brain)
        {
            _brain = brain;
        }

        public void Enter()
        {
            _brain.SetVisibility(false);

            if (_brain.NpcAnimator != null)
            {
                _brain.NpcAnimator.SetWalking(false, -1f);
                _brain.NpcAnimator.SetHealed(false);
            }

            if (_brain.CounterInteractable != null)
            {
                _brain.CounterInteractable.canInteract = false;
            }

            if (_brain.SessionData != null)
            {
                _brain.SessionData.savedNPCState = NPCBrain.NPCStateEnum.HIDDEN;
            }
        }

        public void Update() { }

        public void Exit() { }
    }
}