using UnityEngine;

public class SceneMusic : MonoBehaviour
{
    [Header("Música da Cena")]
    public SoundDataSO musicData;

    private void Start()
    {
        if (musicData != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(musicData);
        }
    }
}