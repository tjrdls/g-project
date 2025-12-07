using UnityEngine;
using UnityEngine.UI;

public class HPBarConnector : MonoBehaviour
{
    public StatsComponent statsComponent; // 직접 연결
    public Image hpFillImage;

    private void Start()
    {
        if (!statsComponent)
            Debug.LogError("StatsComponent가 연결되지 않았습니다!");

        if (!hpFillImage)
            Debug.LogError("hpFillImage가 Inspector에서 연결되지 않았습니다!");
    }

    private void Update()
    {
        if (statsComponent == null || hpFillImage == null)
            return;

        hpFillImage.fillAmount = statsComponent.CurrentHP / statsComponent.MaxHP;
    }
}
