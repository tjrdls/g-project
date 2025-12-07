using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("References")]
    public SettingsUI settingsUI;   // SettingsUI 프리팹 또는 오브젝트 참조

    public void StartGame()
    {
        SceneManager.LoadScene("Main_GameScene");
    }

    public void OpenSettings()
    {
        if (settingsUI == null)
        {
            Debug.LogError("SettingsUI reference is missing in MainMenuUI!");
            return;
        }

        Debug.Log("Open Settings");

        // 설정창 활성화 + 설정 로드
        settingsUI.Open();
    }

    public void CloseSettings()
    {
        if (settingsUI == null)
        {
            Debug.LogError("SettingsUI reference is missing in MainMenuUI!");
            return;
        }

        Debug.Log("Close Settings");

        // 설정창 비활성화
        settingsUI.Close();
    }

    public void QuitGame()
    {
        Debug.Log("Game Quit");
        Application.Quit();
    }
}


