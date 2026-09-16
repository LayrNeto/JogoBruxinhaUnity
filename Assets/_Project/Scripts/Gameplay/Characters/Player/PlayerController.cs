using System.Collections;
using JogoBruxinha.Gameplay.GameFlow;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JogoBruxinha.Gameplay.Characters.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EntityTrackerSO _entityTracker;

        [Header("Movement Settings")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _runMultiplier = 1.5f;

        [Header("Dash Settings")]
        [SerializeField] private float _dashSpeed = 6f;
        [SerializeField] private float _dashDuration = 0.12f;
        [SerializeField] private float _dashCooldown = 1f;

        private Rigidbody2D _rb;
        private Coroutine _dashCoroutine;
        private bool _canDash = true;
        private bool _isAutoWalking;

        public Vector2 MovementInput { get; private set; }
        public Vector2 LastDirection { get; private set; } = Vector2.down;
        public bool IsDashing { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsAutoWalking => _isAutoWalking;

        public float MoveSpeed => _moveSpeed;
        public float RunMultiplier => _runMultiplier;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();

            Debug.Assert(_rb != null, $"Missing Rigidbody2D component on {name}", this);
            Debug.Assert(_entityTracker != null, $"Missing _entityTracker reference on {name}", this);
        }

        private void OnEnable()
        {
            if (_entityTracker != null)
            {
                _entityTracker.Player = this;
            }

            if (GameStateManager.Instance != null && GameStateManager.Instance.InputControls != null)
            {
                GameStateManager.Instance.InputControls.Player.Dash.performed += AttemptDash;
            }
        }

        private void OnDisable()
        {
            if (_entityTracker != null && _entityTracker.Player == this)
            {
                _entityTracker.Player = null;
            }

            if (GameStateManager.Instance != null && GameStateManager.Instance.InputControls != null)
            {
                GameStateManager.Instance.InputControls.Player.Dash.performed -= AttemptDash;
            }

            if (_dashCoroutine != null)
            {
                StopCoroutine(_dashCoroutine);
                _dashCoroutine = null;
                IsDashing = false;
                _canDash = true;
            }
        }

        private void Update()
        {
            if (IsDashing) return;

            if (!_isAutoWalking)
            {
                ProcessInputs();
            }

            if (MovementInput.sqrMagnitude > 0f)
            {
                LastDirection = MovementInput.normalized;
            }
        }

        private void FixedUpdate()
        {
            if (IsDashing || _rb == null) return;

            float speed = IsRunning ? _moveSpeed * _runMultiplier : _moveSpeed;
            _rb.linearVelocity = MovementInput.normalized * speed;
        }

        private void ProcessInputs()
        {
            if (GameStateManager.Instance == null || GameStateManager.Instance.InputControls == null) return;

            MovementInput = GameStateManager.Instance.InputControls.Player.Move.ReadValue<Vector2>();
            IsRunning = GameStateManager.Instance.InputControls.Player.Run.IsPressed();
        }

        private void AttemptDash(InputAction.CallbackContext context)
        {
            if (_canDash && MovementInput.sqrMagnitude > 0f && gameObject.activeInHierarchy)
            {
                _dashCoroutine = StartCoroutine(DashRoutine());
            }
        }

        private IEnumerator DashRoutine()
        {
            IsDashing = true;
            _canDash = false;

            _rb.linearVelocity = MovementInput.normalized * _dashSpeed;

            yield return new WaitForSeconds(_dashDuration);
            IsDashing = false;

            yield return new WaitForSeconds(_dashCooldown);
            _canDash = true;
            _dashCoroutine = null;
        }

        public void ChangeIdleDirection(Vector2 newDir)
        {
            LastDirection = newDir;
        }

        public void StartAutoWalk(Vector2 direction)
        {
            _isAutoWalking = true;
            MovementInput = direction;
        }

        public void StopAutoWalk(Vector2 lookDirection)
        {
            _isAutoWalking = false;
            MovementInput = Vector2.zero;
            LastDirection = lookDirection;
        }
    }
}