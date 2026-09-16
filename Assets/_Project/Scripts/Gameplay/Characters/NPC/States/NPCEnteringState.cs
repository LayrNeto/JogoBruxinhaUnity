using JogoBruxinha.Core.StateMachines;

namespace JogoBruxinha.Gameplay.Characters.NPC
{
    public sealed class NPCEnteringState : IState
    {
        private readonly NPCBrain _brain;

        public NPCEnteringState(NPCBrain brain)
        {
            _brain = brain;
        }

        public void Enter()
        {
            _brain.SetVisibility(true);

            if (_brain.SessionData != null)
            {
                _brain.SessionData.hasNPCSpawnedToday = true;
                _brain.SessionData.savedNPCState = NPCBrain.NPCStateEnum.ENTERING;
            }

            if (_brain.NpcAnimator != null)
            {
                _brain.NpcAnimator.SetWalking(true, -1f);
            }

            if (_brain.MovementScript != null)
            {
                _brain.MovementScript.MoveToCounter(() =>
                {
                    if (_brain.CurrentState == this)
                    {
                        _brain.ChangeState(_brain.WaitingState);
                    }
                });
            }
        }

        public void Update() { }

        public void Exit()
        {
            if (_brain.NpcAnimator != null)
            {
                _brain.NpcAnimator.SetWalking(false, -1f);
            }
        }
    }
}