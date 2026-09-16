using UnityEngine;

namespace JogoBruxinha.Gameplay.Characters.Companion
{
    [RequireComponent(typeof(Animator))]
    public sealed class CompanionAnimator : MonoBehaviour
    {
        private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
        private static readonly int IsSleepingHash = Animator.StringToHash("IsSleeping");
        private static readonly int MoveXHash = Animator.StringToHash("MoveX");

        [Header("Parent References")]
        [SerializeField] private CompanionBrain _brain;
        [SerializeField] private CompanionMovement _movement;

        private Animator _animator;
        private float _lastMoveX = 1f;

        private void Awake()
        {
            _animator = GetComponent<Animator>();

            Debug.Assert(_animator != null, $"Missing Animator component on {name}", this);
            Debug.Assert(_brain != null, $"Missing _brain reference on {name}", this);
            Debug.Assert(_movement != null, $"Missing _movement reference on {name}", this);
        }

        private void Update()
        {
            if (_brain == null || _animator == null) return;

            bool isWalking = _brain.CurrentState == _brain.FollowingState;
            bool isSleeping = _brain.CurrentState == _brain.SleepingState;

            _animator.SetBool(IsWalkingHash, isWalking);
            _animator.SetBool(IsSleepingHash, isSleeping);

            if (isWalking)
            {
                Vector2 dir = _movement != null ? _movement.CurrentDirection : Vector2.zero;

                if (Mathf.Abs(dir.x) > 0.01f)
                {
                    float moveX = Mathf.Sign(dir.x);
                    _animator.SetFloat(MoveXHash, moveX);
                    _lastMoveX = moveX;
                }
            }
            else
            {
                _animator.SetFloat(MoveXHash, _lastMoveX);
            }
        }

        public void ForceDirection(float dirX)
        {
            _lastMoveX = Mathf.Sign(dirX);

            if (_animator != null)
            {
                _animator.SetFloat(MoveXHash, _lastMoveX);
            }
        }

        public void SleepForTheNight()
        {
            if (_animator != null)
            {
                _animator.SetBool(IsWalkingHash, false);
                _animator.SetBool(IsSleepingHash, true);
            }

            ForceDirection(-1f);
        }
    }
}