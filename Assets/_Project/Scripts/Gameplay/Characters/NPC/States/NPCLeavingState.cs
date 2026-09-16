using System.Collections;
using JogoBruxinha.Core.StateMachines;
using UnityEngine;

namespace JogoBruxinha.Gameplay.Characters.NPC
{
    public sealed class NPCLeavingState : IState
    {
        private readonly NPCBrain _brain;
        private Coroutine _leaveCoroutine;

        public NPCLeavingState(NPCBrain brain)
        {
            _brain = brain;
        }

        public void Enter()
        {
            if (_brain.SessionData != null)
            {
                _brain.SessionData.savedNPCState = NPCBrain.NPCStateEnum.LEAVING;
            }

            if (_brain.NpcAnimator != null)
            {
                _brain.NpcAnimator.SetHealed(_brain.IsHealed);
            }

            if (_brain.MovementScript != null && _brain.CurrentData != null)
            {
                float targetSpeed = _brain.IsHealed 
                    ? _brain.CurrentData.healedMovementSpeed 
                    : _brain.CurrentData.cursedMovementSpeed;

                _brain.MovementScript.SetSpeed(targetSpeed);
            }

            _leaveCoroutine = _brain.StartCoroutine(LeaveSequence());
        }

        private IEnumerator LeaveSequence()
        {
            yield return new WaitForSeconds(1.5f);

            if (_brain.NpcAnimator != null)
            {
                _brain.NpcAnimator.SetWalking(true, 1f);
            }

            if (_brain.MovementScript != null)
            {
                _brain.MovementScript.MoveToDoor(() =>
                {
                    if (_brain.CurrentState == this)
                    {
                        _brain.ChangeState(_brain.HiddenState);
                    }
                });
            }

            _leaveCoroutine = null;
        }

        public void Update() { }

        public void Exit()
        {
            if (_leaveCoroutine != null && _brain != null)
            {
                _brain.StopCoroutine(_leaveCoroutine);
                _leaveCoroutine = null;
            }
        }
    }
}