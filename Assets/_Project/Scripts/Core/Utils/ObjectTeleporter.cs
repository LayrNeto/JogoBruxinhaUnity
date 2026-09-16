using UnityEngine;

namespace JogoBruxinha.Core.Utils
{
    public sealed class ObjectTeleporter : MonoBehaviour
    {
        public void TeleportTo(Transform targetDestination)
        {
            if (targetDestination)
            {
                transform.position = targetDestination.position;
            }
        }
    }
}
