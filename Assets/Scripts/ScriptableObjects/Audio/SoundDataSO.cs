using UnityEngine;

[CreateAssetMenu(fileName = "NewSoundData", menuName = "Scriptable Objects/Audio/Sound Data")]
public class SoundDataSO : ScriptableObject
{
    public AudioClip clip;
    
    [Range(0f, 1f)] 
    public float volume = 1f;

    [Range(0.1f, 3f)]
    public float pitch = 1f;
}