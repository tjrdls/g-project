using UnityEngine;

public class SettingsValidator
{
    public bool Validate(SettingsData data)
    {
        // 예: 존재하지 않는 해상도 인덱스 체크
        if (data.resolutionIndex < 0 || data.resolutionIndex >= GraphicsManager.Instance.ResolutionCount)
            return false;

        // 볼륨 범위 체크
        if (data.bgmVolume < 0f || data.bgmVolume > 1f) return false;
        if (data.sfxVolume < 0f || data.sfxVolume > 1f) return false;

        // KeyCode는 기본적으로 유효하다고 가정
        return true;
    }
}
