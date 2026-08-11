using UnityEngine;

public class AnimationEventReceiver : MonoBehaviour
{
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}