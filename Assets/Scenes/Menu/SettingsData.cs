[System.Serializable]
public class SettingsData
{
    public int resolutionIndex;
    public float bgmVolume;
    public float sfxVolume;

    public string skillKeyBinding; // Input System JSON 바인딩

    public SettingsData()
    {
        resolutionIndex = 0;
        bgmVolume = 1f;
        sfxVolume = 1f;
        skillKeyBinding = ""; // 기본값 (InputManager에서 Load 시 처리)
    }
}
