using JogoBruxinha.Core.SceneManagement;
using JogoBruxinha.Gameplay.GameFlow;
using UnityEngine;

namespace JogoBruxinha.Gameplay.Environment
{
    public sealed class SpawnPoint : MonoBehaviour
    {
        [Header("Id")]
        [SerializeField] private string _spawnPointID;

        [Header("Dependencies")]
        [SerializeField] private EntityTrackerSO _tracker;
        [SerializeField] private SessionDataSO _sessionData;

        [Header("Player Settings")]
        [SerializeField] private Vector2 _dir;

        [Header("Companion Settings")]
        [SerializeField] private Vector3 _companionOffset = new Vector3(-1f, 0f, 0f);
        [SerializeField] private Vector2 _companionDir;

        public string SpawnPointID => _spawnPointID;

        private void Awake()
        {
            Debug.Assert(!string.IsNullOrEmpty(_spawnPointID), $"Missing _spawnPointID reference on {name}", this);
            Debug.Assert(_tracker != null, $"Missing _tracker reference on {name}", this);
            Debug.Assert(_sessionData != null, $"Missing _sessionData reference on {name}", this);
        }

        private void Start()
        {
            if (FadeManager.Instance == null)
            {
                Debug.LogWarning($"FadeManager.Instance não encontrado ao carregar SpawnPoint em {name}!", this);
                return;
            }

            if (FadeManager.Instance.TargetSpawnID == _spawnPointID)
            {
                if (_spawnPointID == "SAVED_SPAWN")
                {
                    SetupFromSaveData();
                }

                SpawnEntities();
            }
        }

        private void SetupFromSaveData()
        {
            if (_sessionData == null) return;

            transform.position = _sessionData.playerPos;
            _companionOffset = _sessionData.companionPos - transform.position;
        }

        private void SpawnEntities()
        {
            if (_tracker == null) return;

            if (_tracker.Player != null)
            {
                _tracker.Player.transform.position = transform.position;
                _tracker.Player.ChangeIdleDirection(_dir);
            }

            if (_tracker.Companion != null)
            {
                _tracker.Companion.transform.position = transform.position + _companionOffset;
                _tracker.Companion.ChangeIdleDirection(_companionDir);
            }
        }

        private void OnDrawGizmos()
        {
            // Player 
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.3f);

            // Companion
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + _companionOffset, 0.2f);

            // Link Line
            Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
            Gizmos.DrawLine(transform.position, transform.position + _companionOffset);
        }
    }
}