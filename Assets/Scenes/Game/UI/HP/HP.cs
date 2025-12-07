using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    [Header("UI Components")]
    public Image fillImage; // 빨간 체력바 이미지

    // 체력 갱신
    public void SetHealth(float current, float max)
    {
        float ratio = Mathf.Clamp01(current / max);
        fillImage.fillAmount = ratio;
    }
}
