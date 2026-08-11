using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [Tooltip("Tempo em segundos até o objeto se auto-destruir")]
    public float lifetime = 1f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
}