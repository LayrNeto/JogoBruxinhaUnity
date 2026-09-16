using JogoBruxinha.Core.Audio;
using UnityEngine;
using UnityEngine.Events;

namespace JogoBruxinha.Gameplay.Environment
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class TriggerArea : MonoBehaviour
    {
        [Header("Trigger Settings")]
        [SerializeField] private string _targetTag = "Player";
        [SerializeField] private bool _oneShot = true;

        [Header("Audio")]
        [SerializeField] private SoundDataSO _triggerAudio;

        [Header("Events")]
        [SerializeField] private UnityEvent _onTriggerEnterEvent;

        private Collider2D _collider;
        private bool _hasTriggered;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();

            Debug.Assert(_collider != null, $"Missing Collider2D component on {name}", this);
            Debug.Assert(!string.IsNullOrEmpty(_targetTag), $"Missing _targetTag configuration on {name}", this);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_hasTriggered && _oneShot) return;

            if (other.CompareTag(_targetTag))
            {
                if (_triggerAudio != null && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(_triggerAudio);
                }

                _onTriggerEnterEvent?.Invoke();

                if (_oneShot)
                {
                    _hasTriggered = true;

                    if (_collider != null)
                    {
                        _collider.enabled = false;
                    }
                }
            }
        }
    }
}