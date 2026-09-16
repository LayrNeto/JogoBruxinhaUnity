using UnityEngine;

namespace JogoBruxinha.Core.Utils
{
    public sealed class AnimationEventReceiver : MonoBehaviour
    {
        public void DestroySelf()
        {
            Destroy(gameObject);
        }
    }
}