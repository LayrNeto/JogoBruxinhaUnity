using UnityEngine;

public class UIToggler : MonoBehaviour
{
    public bool isActive;

    void Start()
    {
        gameObject.SetActive(isActive);
    }
    public void ToggleState()
    {
        isActive = !isActive;
        gameObject.SetActive(isActive);
    }
}