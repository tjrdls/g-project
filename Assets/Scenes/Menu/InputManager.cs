using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public PlayerInputActions inputActions;

    private const string SkillKeySave = "SkillKeyRebind";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            inputActions = new PlayerInputActions();
            inputActions.Player.Enable();

            LoadRebind();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region Skill 리바인드

    /// <summary>
    /// Skill 키 리바인드 시작
    /// </summary>
    /// <param name="onComplete">완료 시 호출될 콜백 (ESC 취소 시 "cancel")</param>
    public void StartRebindSkill(System.Action<string> onComplete)
    {
        var action = inputActions.Player.Skill;

        action.Disable();
        action.RemoveAllBindingOverrides();

        action.PerformInteractiveRebinding()
            .WithCancelingThrough("<Keyboard>/escape")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(callback =>
            {
                callback.Dispose();
                SaveRebind();
                action.Enable();
                onComplete?.Invoke(action.GetBindingDisplayString());
            })
            .OnCancel(callback =>
            {
                callback.Dispose();
                action.Enable();
                onComplete?.Invoke("cancel");
            })
            .Start();
    }

    /// <summary>
    /// Player에서 Skill 입력 감지용
    /// </summary>
    public InputAction SkillAction => inputActions.Player.Skill;

    #endregion

    #region 저장 / 불러오기 (PlayerPrefs + SettingsRepository 연동 가능)

    /// <summary>
    /// Skill 바인딩을 JSON으로 저장
    /// </summary>
    public void SaveRebind()
    {
        var action = inputActions.Player.Skill;
        string json = action.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(SkillKeySave, json);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 저장된 Skill 바인딩 불러오기
    /// </summary>
    public void LoadRebind()
    {
        if (PlayerPrefs.HasKey(SkillKeySave))
        {
            string json = PlayerPrefs.GetString(SkillKeySave);
            inputActions.Player.Skill.LoadBindingOverridesFromJson(json);
        }
    }

    /// <summary>
    /// SettingsData와 연동하여 바인딩 적용
    /// </summary>
    public void ApplySettingsBinding(SettingsData data)
    {
        if (!string.IsNullOrEmpty(data.skillKeyBinding))
        {
            inputActions.Player.Skill.LoadBindingOverridesFromJson(data.skillKeyBinding);
        }
    }

    /// <summary>
    /// SettingsData에 현재 바인딩 저장
    /// </summary>
    public void SaveBindingToSettings(SettingsData data)
    {
        data.skillKeyBinding = inputActions.Player.Skill.SaveBindingOverridesAsJson();
    }

    #endregion
}
