using UnityEngine;

namespace JogoBruxinha.Core.Utils
{
    public sealed class AutoDestroy : MonoBehaviour
    {
        public float _lifetime = 1f;

        private void Start()
        {
            Destroy(gameObject, _lifetime);
        }
    }
}