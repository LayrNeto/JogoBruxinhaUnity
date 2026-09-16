using System.Collections.Generic;
using JogoBruxinha.Core.Events;
using JogoBruxinha.Gameplay.Environment;
using JogoBruxinha.Gameplay.GameFlow;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JogoBruxinha.Gameplay.Characters.Player
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class InteractionController : MonoBehaviour
    {
        [Header("UI Events")]
        [SerializeField] private GameEventVector3 _showUIEvent;
        [SerializeField] private GameEvent _hideUIEvent;

        private readonly List<Interactable> _interactablesInRange = new List<Interactable>();
        private Interactable _currentTarget;
        private Collider2D _triggerCollider;

        private void Awake()
        {
            _triggerCollider = GetComponent<Collider2D>();

            Debug.Assert(_triggerCollider != null, $"Missing Collider2D component on {name}", this);
            Debug.Assert(_showUIEvent != null, $"Missing _showUIEvent reference on {name}", this);
            Debug.Assert(_hideUIEvent != null, $"Missing _hideUIEvent reference on {name}", this);
        }

        private void Start()
        {
            if (GameStateManager.Instance != null && GameStateManager.Instance.InputControls != null)
            {
                GameStateManager.Instance.InputControls.Player.Interact.performed += TryInteract;
            }
        }

        private void OnDestroy()
        {
            if (GameStateManager.Instance != null && GameStateManager.Instance.InputControls != null)
            {
                GameStateManager.Instance.InputControls.Player.Interact.performed -= TryInteract;
            }
        }

        private void Update()
        {
            UpdateClosestTarget();
        }

        public void TryInteract(InputAction.CallbackContext context)
        {
            if (_currentTarget != null && _currentTarget.IsAvailable())
            {
                Interactable tempTarget = _currentTarget;
                tempTarget.Interact();

                if (!tempTarget.IsAvailable() && _currentTarget == tempTarget)
                {
                    ClearTarget();
                }
            }
        }

        private void UpdateClosestTarget()
        {
            _interactablesInRange.RemoveAll(item => item == null);

            if (_interactablesInRange.Count == 0)
            {
                ClearTarget();
                return;
            }

            Interactable closest = null;
            float minSqrDistance = float.MaxValue;
            Vector2 playerPos = transform.position;

            foreach (Interactable item in _interactablesInRange)
            {
                if (!item.IsAvailable()) continue;

                float sqrDist = ((Vector2)item.transform.position - playerPos).sqrMagnitude;
                if (sqrDist < minSqrDistance)
                {
                    minSqrDistance = sqrDist;
                    closest = item;
                }
            }

            if (closest != _currentTarget)
            {
                if (closest != null) SetNewTarget(closest);
                else ClearTarget();
            }
        }

        private void SetNewTarget(Interactable target)
        {
            if (_currentTarget != null)
            {
                _currentTarget.SetTargeted(false);
            }

            _currentTarget = target;

            if (_currentTarget != null)
            {
                _currentTarget.SetTargeted(true);

                Vector3 uiPos = _currentTarget.transform.position + _currentTarget.uiOffset;
                _showUIEvent?.Raise(uiPos);
            }
            else
            {
                _hideUIEvent?.Raise();
            }
        }

        public void ClearTarget()
        {
            if (_currentTarget != null)
            {
                _currentTarget.SetTargeted(false);
                _currentTarget = null;
                _hideUIEvent?.Raise();
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            Interactable interactable = collision.GetComponent<Interactable>();
            if (interactable != null && !_interactablesInRange.Contains(interactable))
            {
                _interactablesInRange.Add(interactable);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            Interactable interactable = collision.GetComponent<Interactable>();
            if (interactable != null)
            {
                _interactablesInRange.Remove(interactable);
                if (interactable == _currentTarget)
                {
                    ClearTarget();
                }
            }
        }
    }
}