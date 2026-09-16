using UnityEngine;

namespace JogoBruxinha.Gameplay.UI
{
    public sealed class InteractionUI : MonoBehaviour
    {
        [Header("Child Object")]
        [SerializeField] private GameObject _visualObject;

        private void Awake()
        {
            Debug.Assert(_visualObject != null, $"Missing _visualObject reference on {name}", this);
        }

        public void ShowKey(Vector3 position)
        {
            transform.position = position;

            if (_visualObject != null)
            {
                _visualObject.SetActive(true);
            }
        }

        public void HideKey()
        {
            if (_visualObject != null)
            {
                _visualObject.SetActive(false);
            }
        }
    }
}