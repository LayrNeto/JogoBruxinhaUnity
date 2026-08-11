using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    [Header("Child Object")]
    public GameObject visualObject;

    public void ShowKey(Vector3 position)
    {
        transform.position = position; 
        if (visualObject != null) visualObject.SetActive(true);
    }

    public void HideKey()
    {
        if (visualObject != null) visualObject.SetActive(false);
    }
}