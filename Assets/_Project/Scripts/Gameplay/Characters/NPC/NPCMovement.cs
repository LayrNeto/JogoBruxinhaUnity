using System;
using System.Collections;
using UnityEngine;

namespace JogoBruxinha.Gameplay.Characters.NPC
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class NPCMovement : MonoBehaviour
    {
        [Header("Pathing References")]
        [SerializeField] private Transform _doorPoint;
        [SerializeField] private Transform _counterPoint;

        [Header("Speed Settings")]
        [SerializeField] private float _currentSpeed = 2f;

        public float CurrentSpeed => _currentSpeed;

        private Rigidbody2D _rb;
        private Coroutine _moveCoroutine;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();

            Debug.Assert(_rb != null, $"Missing Rigidbody2D component on {name}", this);
            Debug.Assert(_doorPoint != null, $"Missing _doorPoint reference on {name}", this);
            Debug.Assert(_counterPoint != null, $"Missing _counterPoint reference on {name}", this);
        }

        public void SetSpeed(float speed)
        {
            _currentSpeed = speed;
        }

        public void MoveToCounter(Action onArrived)
        {
            if (_counterPoint == null) return;

            StartMoveRoutine(_counterPoint.position, onArrived);
        }

        public void MoveToDoor(Action onArrived)
        {
            if (_doorPoint == null) return;

            StartMoveRoutine(_doorPoint.position, onArrived);
        }

        public void TeleportToCounter()
        {
            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
                _moveCoroutine = null;
            }

            if (_counterPoint != null && _rb != null)
            {
                _rb.position = _counterPoint.position;
            }
        }

        private void StartMoveRoutine(Vector2 targetPos, Action onArrived)
        {
            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
            }

            _moveCoroutine = StartCoroutine(MoveRoutine(targetPos, onArrived));
        }

        private IEnumerator MoveRoutine(Vector2 targetPos, Action onArrived)
        {
            const float stopThresholdSqr = 0.05f * 0.05f;

            while ((targetPos - _rb.position).sqrMagnitude > stopThresholdSqr)
            {
                Vector2 newPos = Vector2.MoveTowards(_rb.position, targetPos, _currentSpeed * Time.fixedDeltaTime);
                _rb.MovePosition(newPos);

                yield return new WaitForFixedUpdate();
            }

            _rb.MovePosition(targetPos);
            _moveCoroutine = null;

            onArrived?.Invoke();
        }

        public Vector2 GetMovementDirection(Vector3 targetPosition)
        {
            return (targetPosition - transform.position).normalized;
        }
    }
}