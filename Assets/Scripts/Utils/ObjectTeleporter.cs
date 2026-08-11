using UnityEngine;

public class ObjectTeleporter : MonoBehaviour
{
    public void TeleportTo(Transform targetDestination)
    {
        if (targetDestination)
        {
            transform.position = targetDestination.position;
        }
    }
}
