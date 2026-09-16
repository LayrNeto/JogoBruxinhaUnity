using UnityEngine;

namespace JogoBruxinha.Gameplay.Characters.Player
{
    [RequireComponent(typeof(Animator))]
    public sealed class PlayerAnimator : MonoBehaviour
    {
        private static readonly int RunMultiplierHash = Animator.StringToHash("RunMultiplier");
        private static readonly int MoveXHash = Animator.StringToHash("MoveX");
        private static readonly int MoveYHash = Animator.StringToHash("MoveY");
        private static readonly int LastMoveXHash = Animator.StringToHash("LastMoveX");
        private static readonly int LastMoveYHash = Animator.StringToHash("LastMoveY");
        private static readonly int SpeedHash = Animator.StringToHash("Speed");

        [Header("Player Reference")]
        [SerializeField] private PlayerController _player;

        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();

            Debug.Assert(_animator != null, $"Missing Animator component on {name}", this);
            Debug.Assert(_player != null, $"Missing _player reference on {name}", this);
        }

        private void Update()
        {
            if (_player == null || _animator == null) return;
            if (_player.IsDashing) return;

            Vector2 animInput = _player.MovementInput;
            Vector2 animLastDir = _player.LastDirection;

            if (Mathf.Abs(animInput.x) > 0.01f) animInput.y = 0f;
            if (Mathf.Abs(animLastDir.x) > 0.01f) animLastDir.y = 0f;

            animInput = animInput.normalized;
            animLastDir = animLastDir.normalized;

            float multiplier = _player.IsRunning ? 1.8f : 1f;

            _animator.SetFloat(RunMultiplierHash, multiplier);
            _animator.SetFloat(MoveXHash, animInput.x);
            _animator.SetFloat(MoveYHash, animInput.y);
            _animator.SetFloat(LastMoveXHash, animLastDir.x);
            _animator.SetFloat(LastMoveYHash, animLastDir.y);
            _animator.SetFloat(SpeedHash, _player.MovementInput.sqrMagnitude);
        }
    }
}