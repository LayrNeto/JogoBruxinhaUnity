using JogoBruxinha.Core.StateMachines;
using UnityEngine;

namespace JogoBruxinha.Gameplay.Characters.Companion
{
    public sealed class CompanionFollowingState : IState
    {
        private readonly CompanionBrain _brain;

        public CompanionFollowingState(CompanionBrain brain)
        {
            _brain = brain;
        }

        public void Enter()
        {
            _brain.MovementScript.StartFollowing();
        }

        public void Update()
        {
            if (_brain.TargetPlayer == null) return;

            Vector2 diff = (Vector2)_brain.TargetPlayer.position - (Vector2)_brain.transform.position;
            float sqrDistance = diff.sqrMagnitude;

            float sqrDistanceToStop = _brain.DistanceToStop * _brain.DistanceToStop;
            if (sqrDistance <= sqrDistanceToStop)
            {
                _brain.ChangeState(_brain.IdleState);
            }
        }

        public void Exit()
        {
            _brain.MovementScript.StopFollowing();
        }
    }
}