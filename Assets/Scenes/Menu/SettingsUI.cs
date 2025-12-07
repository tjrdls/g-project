using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class SettingsUI : MonoBehaviour
{
    [Header("Sliders")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    [Header("Buttons")]
    public Button skillKeyButton;
    public Button applyButton;
    public TMP_Text skillKeyText;

    // 원래 저장된 값 (닫기 시 복원)
    private float originalBgm;
    private float originalSfx;

    private void Start()
    {
        LoadOriginalValues();

        // 슬라이더 이벤트 (즉시 미리듣기만)
        bgmSlider.onValueChanged.AddListener(value =>
        {
            SoundManager.Instance.PreviewBGM(value);
        });

        sfxSlider.onValueChanged.AddListener(value =>
        {
            SoundManager.Instance.PreviewSFX(value);
        });

        // 버튼 이벤트
        skillKeyButton.onClick.AddListener(OnClick_ChangeSkillKey);
        applyButton.onClick.AddListener(OnClick_Apply);
    }

    private void OnEnable()
    {
        LoadOriginalValues();
    }

    // ---------------------------------------------------------
    // 저장된 원본 값 로드 + UI 표시
    // ---------------------------------------------------------
    private void LoadOriginalValues()
    {
        originalBgm = PlayerPrefs.GetFloat("BGMVolume", 1f);
        originalSfx = PlayerPrefs.GetFloat("SFXVolume", 1f);

        bgmSlider.value = originalBgm;
        sfxSlider.value = originalSfx;

        // 현재 스킬 키 UI 업데이트
        skillKeyText.text = InputManager.Instance.SkillAction.GetBindingDisplayString(0);
    }

    // ---------------------------------------------------------
    // 스킬 키 변경 (리바인드)
    // ---------------------------------------------------------
    private void OnClick_ChangeSkillKey()
    {
        skillKeyText.text = "Press a key...";

        InputManager.Instance.StartRebindSkill(result =>
        {
            if (result == "cancel")
            {
                skillKeyText.text = InputManager.Instance.SkillAction.GetBindingDisplayString(0);
                return;
            }

            skillKeyText.text = InputManager.Instance.SkillAction.GetBindingDisplayString(0);
        });
    }

    // ---------------------------------------------------------
    // 적용 버튼 → 진짜로 저장 + 반영
    // ---------------------------------------------------------
    private void OnClick_Apply()
    {
        SoundManager.Instance.ApplyVolumes(bgmSlider.value, sfxSlider.value);

        originalBgm = bgmSlider.value;
        originalSfx = sfxSlider.value;

        Debug.Log("설정 저장 완료!");
    }

    // ---------------------------------------------------------
    // 설정창 열기 (Skill 키 텍스트 갱신)
    // ---------------------------------------------------------
    public void Open()
    {
        gameObject.SetActive(true);
        skillKeyText.text = InputManager.Instance.SkillAction.GetBindingDisplayString(0);
    }

    // ---------------------------------------------------------
    // 닫기 → 즉시 미리듣기 했던 값 모두 원복
    // ---------------------------------------------------------
    public void Close()
    {
        // Mixer 복구
        SoundManager.Instance.PreviewBGM(originalBgm);
        SoundManager.Instance.PreviewSFX(originalSfx);

        // UI 복원
        bgmSlider.value = originalBgm;
        sfxSlider.value = originalSfx;

        gameObject.SetActive(false);
    }
}
