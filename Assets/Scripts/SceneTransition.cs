using System.Collections;
//using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    [Header("Configs")]
    public float fadeOutTime = 1f;
    public float fadeInTime = 1f;
    public string sceneName;
    public string spawnDestinyID;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !FadeManager.Instance.isFading)
        {
            FadeManager.Instance.StartTransition(sceneName, spawnDestinyID, fadeOutTime, fadeInTime);
        }
    }
}
