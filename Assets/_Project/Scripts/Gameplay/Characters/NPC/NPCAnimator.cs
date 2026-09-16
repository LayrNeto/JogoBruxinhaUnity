using UnityEngine;

namespace JogoBruxinha.Gameplay.Characters.NPC
{
    [RequireComponent(typeof(Animator))]
    public sealed class NPCAnimator : MonoBehaviour
    {
        private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
        private static readonly int IsHealedHash = Animator.StringToHash("IsHealed");
        private static readonly int MoveYHash = Animator.StringToHash("MoveY");

        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            Debug.Assert(_animator != null, $"Missing Animator component on {name}", this);
        }

        public void SetController(RuntimeAnimatorController controller)
        {
            if (_animator != null)
            {
                _animator.runtimeAnimatorController = controller;
            }
        }

        public void SetWalking(bool isWalking, float directionY = -1f)
        {
            if (_animator != null)
            {
                _animator.SetBool(IsWalkingHash, isWalking);
                _animator.SetFloat(MoveYHash, directionY);
            }
        }

        public void SetHealed(bool isHealed)
        {
            if (_animator != null)
            {
                _animator.SetBool(IsHealedHash, isHealed);
            }
        }

        public void SetActive(bool active)
        {
            if (_animator != null)
            {
                _animator.enabled = active;
            }
        }
    }
}