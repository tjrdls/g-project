using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadSavedVolumes(); // 시작 시 저장된 값 불러오기
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ---------------------------------------------------------
    // 저장된 값 불러오기
    // ---------------------------------------------------------
    public void LoadSavedVolumes()
    {
        float bgm = PlayerPrefs.GetFloat("BGMVolume", 1f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 1f);

        SetMixerOnly_BGM(bgm);
        SetMixerOnly_SFX(sfx);
    }

    // ---------------------------------------------------------
    // 즉시 미리듣기 ? UI에서 실시간으로 불리는 부분
    // ---------------------------------------------------------
    public void PreviewBGM(float value) => SetMixerOnly_BGM(value);
    public void PreviewSFX(float value) => SetMixerOnly_SFX(value);

    // ---------------------------------------------------------
    // Apply ? 실제 저장되는 부분 (버튼 눌렀을 때만!)
    // ---------------------------------------------------------
    public void ApplyVolumes(float bgm, float sfx)
    {
        PlayerPrefs.SetFloat("BGMVolume", bgm);
        PlayerPrefs.SetFloat("SFXVolume", sfx);
        PlayerPrefs.Save();

        // mixer에도 반영
        SetMixerOnly_BGM(bgm);
        SetMixerOnly_SFX(sfx);

        Debug.Log($"[SoundManager] Volume Applied: BGM={bgm}, SFX={sfx}");
    }

    // ---------------------------------------------------------
    // Mixer 직접 제어 (미리듣기 & 실제 적용에 모두 사용)
    // ---------------------------------------------------------
    public void SetMixerOnly_BGM(float value)
    {
        if (value < 0.0001f) value = 0.0001f;
        audioMixer.SetFloat("BGMVolume", Mathf.Log10(value) * 20f);
    }

    public void SetMixerOnly_SFX(float value)
    {
        if (value < 0.0001f) value = 0.0001f;
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20f);
    }

    // ---------------------------------------------------------
    // BGM / SFX 재생 (기존 코드 유지)
    // ---------------------------------------------------------
    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource == null) return;
        if (bgmSource.clip == clip) return;

        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }
}
