using JogoBruxinha.Core.StateMachines;
using UnityEngine;

namespace JogoBruxinha.Gameplay.Characters.Companion
{
    public sealed class CompanionSleepingState : IState
    {
        private readonly CompanionBrain _brain;

        public CompanionSleepingState(CompanionBrain brain)
        {
            _brain = brain;
        }

        public void Enter()
        {
            _brain.MovementScript.StopFollowing();
        }

        public void Update()
        {
            if (_brain.TargetPlayer == null) return;

            Vector2 diff = (Vector2)_brain.TargetPlayer.position - (Vector2)_brain.transform.position;
            float sqrDistance = diff.sqrMagnitude;

            float sqrDistanceToStopSleeping = _brain.DistanceToStopSleeping * _brain.DistanceToStopSleeping;
            if (sqrDistance > sqrDistanceToStopSleeping)
            {
                _brain.ChangeState(_brain.IdleState);
            }
        }

        public void Exit() { }
    }
}