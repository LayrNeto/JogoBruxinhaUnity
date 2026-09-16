using UnityEngine;
using UnityEngine.Events;

namespace JogoBruxinha.Gameplay.Environment
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class Interactable : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private bool _oneShot = false;
        public bool canInteract = true;

        [HideInInspector]
        public bool isTimeAllowed = true;

        [Header("UI Key Position")]
        public Vector3 uiOffset = new Vector3(0f, 1.5f, 0f);

        [Header("Events")]
        [SerializeField] private UnityEvent _onInteract;
        [SerializeField] private UnityEvent _onTargeted;
        [SerializeField] private UnityEvent _onUntargeted;

        private Collider2D _collider;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            Debug.Assert(_collider != null, $"Missing Collider2D component on {name}", this);
        }

        public bool IsAvailable()
        {
            return canInteract && isTimeAllowed;
        }

        public void Interact()
        {
            if (!canInteract) return;

            _onInteract?.Invoke();

            if (_oneShot)
            {
                canInteract = false;

                if (_collider != null)
                {
                    _collider.enabled = false;
                }
            }
        }

        public void SetTargeted(bool targeted)
        {
            if (!canInteract) return;

            if (targeted)
            {
                _onTargeted?.Invoke();
            }
            else
            {
                _onUntargeted?.Invoke();
            }
        }

        public void SetInteractionState(bool s)
        {
            canInteract = s;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + uiOffset, 0.2f);
        }
    }
}