using UnityEngine;
using UnityEngine.AI;

namespace JogoBruxinha.Gameplay.Characters.Companion
{
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class CompanionMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private Transform _target;
        [SerializeField] private float _speed = 2f;

        [Header("Collision")]
        [SerializeField] private float _radius = 0.4f;
        [SerializeField] private float _centerHeight = 0.4f;
        [SerializeField] private LayerMask _obstaclesLayer;

        public Transform Target
        {
            get => _target;
            set => _target = value;
        }

        public float Speed => _speed;
        public Vector2 CurrentDirection { get; private set; }

        private NavMeshAgent _agent;
        private Color _debugCircleCast;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();

            Debug.Assert(_agent != null, $"Missing NavMeshAgent component on {name}", this);
            Debug.Assert(_target != null, $"Missing _target reference on {name}", this);

            _agent.updateRotation = false;
            _agent.updateUpAxis = false;
            _agent.speed = _speed;

            _debugCircleCast = Color.green;
        }

        private void Update()
        {
            if (!_target) return;

            Vector2 myPos = (Vector2)transform.position + new Vector2(0f, _centerHeight);
            Vector2 targetPos = (Vector2)_target.position;

            Vector2 diff = targetPos - myPos;
            float distance = diff.magnitude;
            Vector2 dir = (distance > 0.0001f) ? diff / distance : Vector2.zero;

            RaycastHit2D hit = Physics2D.CircleCast(myPos, _radius, dir, distance, _obstaclesLayer);

            _debugCircleCast = (hit.collider != null) ? Color.red : Color.green;

            if (hit.collider == null)
            {
                _agent.isStopped = true;
                transform.position = Vector2.MoveTowards(transform.position, targetPos, _speed * Time.deltaTime);
                CurrentDirection = dir;
            }
            else
            {
                _agent.isStopped = false;
                _agent.SetDestination(targetPos);
                CurrentDirection = _agent.velocity.normalized;
            }
        }

        public void StartFollowing()
        {
            enabled = true;
        }

        public void StopFollowing()
        {
            if (_agent != null && _agent.isOnNavMesh)
            {
                _agent.isStopped = true;
            }

            enabled = false;
        }

        public void DisableMovement()
        {
            if (_agent != null)
            {
                _agent.enabled = false;
            }

            enabled = false;
        }

        public void TeleportTo(Vector2 newPosition)
        {
            if (_agent != null && _agent.isActiveAndEnabled)
            {
                _agent.Warp(newPosition);
            }
            else
            {
                transform.position = newPosition;
            }
        }

        private void OnDrawGizmos()
        {
            Vector2 center = (Vector2)transform.position + new Vector2(0f, _centerHeight);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(center, _radius);

            if (_target != null)
            {
                Vector2 targetCenter = (Vector2)_target.position;
                Vector2 direcao = (targetCenter - center).normalized;
                Vector2 perpendicular = new Vector2(-direcao.y, direcao.x) * _radius;

                Gizmos.color = _debugCircleCast;

                Gizmos.DrawLine(center + perpendicular, targetCenter + perpendicular);
                Gizmos.DrawLine(center - perpendicular, targetCenter - perpendicular);
            }
        }
    }
}