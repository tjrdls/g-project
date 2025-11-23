using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class MainMenuButton : MonoBehaviour
{
    public GameObject optionsPanel;

    public void StartGame()
    {
        SceneManager.LoadScene("Main_GameScene");
    }

    public void OpenSettings()
    {
        Debug.Log("Open Setting");
        optionsPanel.SetActive(true);
 
    }

    public void CloseSettings()
    {
        Debug.Log("Close Setting");
        optionsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("Game Quit");
        Application.Quit();
    }
    
}
