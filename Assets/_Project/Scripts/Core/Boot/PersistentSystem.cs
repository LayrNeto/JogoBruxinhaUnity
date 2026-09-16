using UnityEngine;

namespace JogoBruxinha.Core.Boot
{
    public sealed class PersistentSystem : MonoBehaviour
    {
        public static PersistentSystem Instance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}