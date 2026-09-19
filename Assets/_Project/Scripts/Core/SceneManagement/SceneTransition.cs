using UnityEngine;

namespace JogoBruxinha.Core.SceneManagement
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class SceneTransition : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _fadeOutTime = 1f;
        [SerializeField] private float _fadeInTime = 1f;
        [SerializeField] private string _sceneName;
        [SerializeField] private string _spawnDestinyID;

        public string TargetSceneName => _sceneName;

        private void Awake()
        {
            Debug.Assert(!string.IsNullOrEmpty(_sceneName), $"Missing sceneName reference on {name}", this);
            Debug.Assert(!string.IsNullOrEmpty(_spawnDestinyID), $"Missing spawnDestinyID reference on {name}", this);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") && FadeManager.Instance != null && !FadeManager.Instance.IsFading)
            {
                FadeManager.Instance.StartTransition(_sceneName, _spawnDestinyID, _fadeOutTime, _fadeInTime);
            }
        }
    }
}
