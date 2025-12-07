using UnityEngine;

public class BGMSetter : MonoBehaviour
{
    public AudioClip bgm;

    void Start() 
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayBGM(bgm); 
    }

    void OnEnable()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayBGM(bgm);
    }

}
