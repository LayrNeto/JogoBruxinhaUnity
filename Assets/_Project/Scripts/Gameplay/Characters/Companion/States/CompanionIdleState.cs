using JogoBruxinha.Core.StateMachines;
using UnityEngine;

namespace JogoBruxinha.Gameplay.Characters.Companion
{
    public sealed class CompanionIdleState : IState
    {
        private readonly CompanionBrain _brain;
        private float _idleTimer;

        public CompanionIdleState(CompanionBrain brain)
        {
            _brain = brain;
        }

        public void Enter()
        {
            _idleTimer = 0f;
            _brain.MovementScript.StopFollowing();
        }

        public void Update()
        {
            if (_brain.TargetPlayer == null) return;

            Vector2 diff = (Vector2)_brain.TargetPlayer.position - (Vector2)_brain.transform.position;
            float sqrDistance = diff.sqrMagnitude;

            float sqrDistanceToFollow = _brain.DistanceToStartFollowing * _brain.DistanceToStartFollowing;
            if (sqrDistance > sqrDistanceToFollow)
            {
                _brain.ChangeState(_brain.FollowingState);
                return;
            }

            _idleTimer += Time.deltaTime;

            float sqrDistanceToSleep = _brain.DistanceToStopSleeping * _brain.DistanceToStopSleeping;
            if (_idleTimer >= _brain.TimeToFallAsleep && sqrDistance <= sqrDistanceToSleep)
            {
                _brain.ChangeState(_brain.SleepingState);
            }
        }

        public void Exit() { }
    }
}