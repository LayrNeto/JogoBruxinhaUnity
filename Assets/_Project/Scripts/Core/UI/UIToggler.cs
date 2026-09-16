using UnityEngine;

namespace JogoBruxinha.Core.UI
{
    public sealed class UIToggler : MonoBehaviour
    {
        [Header("Target UI (Leave empty to use this GameObject)")]
        [SerializeField] private GameObject _targetUI;

        [Header("Initial State")]
        [SerializeField] private bool _startsActive = false;

        private GameObject Target => _targetUI != null ? _targetUI : gameObject;

        public bool IsActive => Target.activeSelf;

        private void Awake()
        {
            if (Target.activeSelf != _startsActive)
            {
                Target.SetActive(_startsActive);
            }
        }

        public void ToggleState()
        {
            Target.SetActive(!Target.activeSelf);
        }

        public void SetState(bool active)
        {
            Target.SetActive(active);
        }
    }
}