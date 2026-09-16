using JogoBruxinha.Core.StateMachines;
using JogoBruxinha.Gameplay.Environment;
using JogoBruxinha.Gameplay.GameFlow;
using UnityEngine;

namespace JogoBruxinha.Gameplay.Characters.Companion
{
    [RequireComponent(typeof(Interactable))]
    [RequireComponent(typeof(CompanionMovement))]
    public sealed class CompanionBrain : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EntityTrackerSO _entityTracker;
        [SerializeField] private SessionDataSO _sessionData;
        [SerializeField] private Transform _targetPlayer;
        [SerializeField] private CompanionAnimator _childAnimator;

        [Header("Distance Rules")]
        [SerializeField] private float _distanceToStartFollowing = 4f;
        [SerializeField] private float _distanceToStop = 1.5f;

        [Header("Sleep Rules")]
        [SerializeField] private float _timeToFallAsleep = 5f;
        [SerializeField] private float _distanceToStopSleeping = 2f;

        [Header("Target Transforms")]
        [SerializeField] private Transform _bedTransform;

        public Transform TargetPlayer => _targetPlayer;
        public float DistanceToStartFollowing => _distanceToStartFollowing;
        public float DistanceToStop => _distanceToStop;
        public float TimeToFallAsleep => _timeToFallAsleep;
        public float DistanceToStopSleeping => _distanceToStopSleeping;
        public Transform BedTransform => _bedTransform;

        public IState IdleState { get; private set; }
        public IState FollowingState { get; private set; }
        public IState SleepingState { get; private set; }
        public IState CurrentState { get; private set; }

        public CompanionMovement MovementScript { get; private set; }
        private Interactable _interactable;
        private bool _isMovementBlocked;

        private void Awake()
        {
            MovementScript = GetComponent<CompanionMovement>();
            _interactable = GetComponent<Interactable>();

            Debug.Assert(_entityTracker != null, $"Missing _entityTracker reference on {name}", this);
            Debug.Assert(_sessionData != null, $"Missing _sessionData reference on {name}", this);
            Debug.Assert(_targetPlayer != null, $"Missing _targetPlayer reference on {name}", this);
            Debug.Assert(_childAnimator != null, $"Missing _childAnimator reference on {name}", this);
            Debug.Assert(_bedTransform != null, $"Missing _bedTransform reference on {name}", this);
            Debug.Assert(MovementScript != null, $"Missing CompanionMovement component on {name}", this);
            Debug.Assert(_interactable != null, $"Missing Interactable component on {name}", this);

            IdleState = new CompanionIdleState(this);
            FollowingState = new CompanionFollowingState(this);
            SleepingState = new CompanionSleepingState(this);
        }

        private void OnEnable()
        {
            if (_entityTracker != null)
            {
                _entityTracker.Companion = this;
            }
        }

        private void OnDisable()
        {
            if (_entityTracker != null && _entityTracker.Companion == this)
            {
                _entityTracker.Companion = null;
            }
        }

        private void Start()
        {
            if (_sessionData != null && _sessionData.isNight)
            {
                GoSleepForTheNight();
                return;
            }

            ChangeState(IdleState);
        }

        private void Update()
        {
            if (_isMovementBlocked || _targetPlayer == null) return;

            CurrentState?.Update();
        }

        public void ChangeState(IState newState)
        {
            if (newState == null || CurrentState == newState) return;

            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState.Enter();
        }

        public void ChangeIdleDirection(Vector2 newDir)
        {
            if (_childAnimator != null)
            {
                _childAnimator.ForceDirection(newDir.x);
            }
            else
            {
                Debug.LogWarning($"CompanionAnimator não referenciado em {name}!", this);
            }
        }

        public void GoSleepForTheNight()
        {
            CurrentState = SleepingState;

            if (MovementScript != null) MovementScript.DisableMovement();
            if (_childAnimator != null) _childAnimator.SleepForTheNight();

            if (_bedTransform != null)
            {
                transform.position = _bedTransform.position;
            }

            if (_interactable != null)
            {
                _interactable.canInteract = false;
            }

            enabled = false;
        }
    }
}